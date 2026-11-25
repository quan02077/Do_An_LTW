using DoAnLTW.Data;
using DoAnLTW.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Services;

// tin nhắn gửi tới trình duyệt (SignalR/REST, JSON camelCase). TenNhanVien chỉ có ở phía nhân viên, phía khách luôn là null.
public record SupportMessageDto(long MaTn, int MaKh, string TenKhach, bool TuKhach, string NoiDung, DateTime GuiLuc, string? TenNhanVien);

// một dòng trong hộp thư của nhân viên
public record SupportConversationDto(int MaKh, string TenKhach, string? Email, string TinCuoi, bool TinCuoiTuKhach, DateTime CapNhatLuc, int ChuaDoc);

public interface ISupportChatService
{
    Task<int?> GetCustomerIdAsync(int maTk, CancellationToken ct = default);
    Task<IReadOnlyList<SupportMessageDto>> GetCustomerHistoryAsync(int maKh, int take, CancellationToken ct = default);
    Task<int> CountUnreadForCustomerAsync(int maKh, CancellationToken ct = default);
    Task<int> CountUnreadForStaffAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SupportConversationDto>> ListConversationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SupportMessageDto>?> GetConversationAsync(int maKh, int take, CancellationToken ct = default);
    Task<ServiceResult<SupportMessageDto>> CustomerSendAsync(int maKh, string text, CancellationToken ct = default);
    Task<ServiceResult<SupportMessageDto>> StaffSendAsync(int maTkNhanVien, int maKh, string text, CancellationToken ct = default);
    Task MarkReadByCustomerAsync(int maKh, CancellationToken ct = default);
    Task MarkReadByStaffAsync(int maKh, CancellationToken ct = default);
}

public class SupportChatService(DbContext_TuiXach db) : ISupportChatService
{
    public async Task<int?> GetCustomerIdAsync(int maTk, CancellationToken ct = default) =>
        await db.KhachHangs.AsNoTracking().Where(x => x.maTK == maTk).Select(x => (int?)x.maKH).SingleOrDefaultAsync(ct);

    public async Task<IReadOnlyList<SupportMessageDto>> GetCustomerHistoryAsync(int maKh, int take, CancellationToken ct = default)
    {
        var rows = await db.TinNhans.AsNoTracking()
            .Where(t => t.CuocTroChuyen!.maKH == maKh)
            .OrderByDescending(t => t.guiLuc).ThenByDescending(t => t.maTN)
            .Take(take)
            .Select(t => new { t.maTN, t.tuKhach, t.noiDung, t.guiLuc, ten = t.CuocTroChuyen!.KhachHang!.hoTen })
            .ToListAsync(ct);

        // phía khách không bao giờ thấy tên tài khoản nhân viên
        return rows.AsEnumerable().Reverse()
            .Select(r => new SupportMessageDto(r.maTN, maKh, r.ten, r.tuKhach, r.noiDung, r.guiLuc, null))
            .ToList();
    }

    public Task<int> CountUnreadForCustomerAsync(int maKh, CancellationToken ct = default) =>
        db.TinNhans.CountAsync(t => t.CuocTroChuyen!.maKH == maKh && !t.tuKhach && !t.daDoc, ct);

    public Task<int> CountUnreadForStaffAsync(CancellationToken ct = default) =>
        db.TinNhans.CountAsync(t => t.tuKhach && !t.daDoc, ct);

    public async Task<IReadOnlyList<SupportConversationDto>> ListConversationsAsync(CancellationToken ct = default)
    {
        var convs = await db.CuocTroChuyens.AsNoTracking()
            .OrderByDescending(c => c.capNhatLuc)
            .Select(c => new
            {
                c.maKH,
                c.KhachHang!.hoTen,
                c.KhachHang.email,
                c.capNhatLuc,
                last = c.TinNhans.OrderByDescending(t => t.guiLuc).ThenByDescending(t => t.maTN)
                    .Select(t => new { t.noiDung, t.tuKhach }).FirstOrDefault(),
                chuaDoc = c.TinNhans.Count(t => t.tuKhach && !t.daDoc)
            })
            .ToListAsync(ct);

        return convs.Select(c => new SupportConversationDto(
            c.maKH, c.hoTen, c.email, Snippet(c.last?.noiDung), c.last?.tuKhach ?? true, c.capNhatLuc, c.chuaDoc)).ToList();
    }

    public async Task<IReadOnlyList<SupportMessageDto>?> GetConversationAsync(int maKh, int take, CancellationToken ct = default)
    {
        var ten = await db.KhachHangs.AsNoTracking().Where(k => k.maKH == maKh).Select(k => k.hoTen).SingleOrDefaultAsync(ct);
        if (ten == null) return null;

        var rows = await db.TinNhans.AsNoTracking()
            .Where(t => t.CuocTroChuyen!.maKH == maKh)
            .OrderByDescending(t => t.guiLuc).ThenByDescending(t => t.maTN)
            .Take(take)
            .Select(t => new
            {
                t.maTN, t.tuKhach, t.noiDung, t.guiLuc,
                nhanVien = t.maTKNhanVien == null ? null : db.TaiKhoans.Where(a => a.maTK == t.maTKNhanVien).Select(a => a.tenDangNhap).FirstOrDefault()
            })
            .ToListAsync(ct);

        return rows.AsEnumerable().Reverse()
            .Select(r => new SupportMessageDto(r.maTN, maKh, ten, r.tuKhach, r.noiDung, r.guiLuc, r.nhanVien))
            .ToList();
    }

    public async Task<ServiceResult<SupportMessageDto>> CustomerSendAsync(int maKh, string text, CancellationToken ct = default)
    {
        var clean = Normalize(text, out var error);
        if (clean == null) return ServiceResult<SupportMessageDto>.Fail(error!);

        var customer = await db.KhachHangs.AsNoTracking().Where(k => k.maKH == maKh).Select(k => new { k.maKH, k.hoTen }).SingleOrDefaultAsync(ct);
        if (customer == null) return ServiceResult<SupportMessageDto>.Fail("Không tìm thấy hồ sơ khách hàng.");

        var conv = await GetOrCreateConversationAsync(maKh, ct);
        var msg = new TinNhan { CuocTroChuyen = conv, tuKhach = true, noiDung = clean, guiLuc = DateTime.Now, daDoc = false };
        db.TinNhans.Add(msg);
        conv.capNhatLuc = msg.guiLuc;
        await db.SaveChangesAsync(ct);

        return ServiceResult<SupportMessageDto>.Ok(new SupportMessageDto(msg.maTN, maKh, customer.hoTen, true, msg.noiDung, msg.guiLuc, null));
    }

    public async Task<ServiceResult<SupportMessageDto>> StaffSendAsync(int maTkNhanVien, int maKh, string text, CancellationToken ct = default)
    {
        var clean = Normalize(text, out var error);
        if (clean == null) return ServiceResult<SupportMessageDto>.Fail(error!);

        var customer = await db.KhachHangs.AsNoTracking().Where(k => k.maKH == maKh).Select(k => new { k.maKH, k.hoTen }).SingleOrDefaultAsync(ct);
        if (customer == null) return ServiceResult<SupportMessageDto>.Fail("Không tìm thấy khách hàng này.");

        var staffName = await db.TaiKhoans.AsNoTracking().Where(a => a.maTK == maTkNhanVien).Select(a => a.tenDangNhap).SingleOrDefaultAsync(ct);

        var conv = await GetOrCreateConversationAsync(maKh, ct);
        var msg = new TinNhan { CuocTroChuyen = conv, tuKhach = false, maTKNhanVien = maTkNhanVien, noiDung = clean, guiLuc = DateTime.Now, daDoc = false };
        db.TinNhans.Add(msg);
        conv.capNhatLuc = msg.guiLuc;
        await db.SaveChangesAsync(ct);

        return ServiceResult<SupportMessageDto>.Ok(new SupportMessageDto(msg.maTN, maKh, customer.hoTen, false, msg.noiDung, msg.guiLuc, staffName));
    }

    public Task MarkReadByCustomerAsync(int maKh, CancellationToken ct = default) =>
        db.TinNhans.Where(t => t.CuocTroChuyen!.maKH == maKh && !t.tuKhach && !t.daDoc)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.daDoc, true), ct);

    public Task MarkReadByStaffAsync(int maKh, CancellationToken ct = default) =>
        db.TinNhans.Where(t => t.CuocTroChuyen!.maKH == maKh && t.tuKhach && !t.daDoc)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.daDoc, true), ct);

    private async Task<CuocTroChuyen> GetOrCreateConversationAsync(int maKh, CancellationToken ct)
    {
        var conv = await db.CuocTroChuyens.SingleOrDefaultAsync(c => c.maKH == maKh, ct);
        if (conv != null) return conv;

        conv = new CuocTroChuyen { maKH = maKh, capNhatLuc = DateTime.Now };
        db.CuocTroChuyens.Add(conv);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // hai tab của cùng một khách gửi tin đầu tiên cùng lúc: chỉ số duy nhất theo khách chặn bản thứ hai, lấy bản đã có
            db.Entry(conv).State = EntityState.Detached;
            conv = await db.CuocTroChuyens.SingleAsync(c => c.maKH == maKh, ct);
        }
        return conv;
    }

    // chuẩn hóa nội dung: bỏ ký tự điều khiển (trừ xuống dòng), gom nhiều dòng trống, cắt khoảng trắng, kiểm tra độ dài
    internal static string? Normalize(string? text, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            error = "Vui lòng nhập nội dung tin nhắn.";
            return null;
        }

        var sb = new System.Text.StringBuilder(text.Length);
        foreach (var ch in text.Replace("\r\n", "\n").Replace('\r', '\n'))
        {
            if (ch == '\n' || ch == '\t' || !char.IsControl(ch)) sb.Append(ch == '\t' ? ' ' : ch);
        }

        var clean = System.Text.RegularExpressions.Regex.Replace(sb.ToString().Trim(), "\n{3,}", "\n\n");
        if (clean.Length == 0)
        {
            error = "Vui lòng nhập nội dung tin nhắn.";
            return null;
        }

        if (clean.Length > TinNhan.MaxNoiDung)
        {
            error = $"Tin nhắn tối đa {TinNhan.MaxNoiDung} ký tự.";
            return null;
        }

        return clean;
    }

    private static string Snippet(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var oneLine = text.Replace('\n', ' ');
        return oneLine.Length <= 80 ? oneLine : oneLine[..80] + "…";
    }
}
