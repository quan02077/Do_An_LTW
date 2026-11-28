using DoAnLTW.Data;
using DoAnLTW.Models;
using DoAnLTW.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DoAnLTW.Services;

public interface IAccountService
{
    Task<TaiKhoan?> ValidateCredentialsAsync(string tenDangNhap, string matKhau, CancellationToken cancellationToken = default);
    Task<ServiceResult<TaiKhoan>> RegisterAsync(RegisterViewModel request, CancellationToken cancellationToken = default);
    Task<ProfileViewModel?> GetProfileAsync(int maTk, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateProfileAsync(int maTk, ProfileViewModel request, CancellationToken cancellationToken = default);
    Task<ServiceResult> ChangePasswordAsync(int maTk, string matKhauHienTai, string matKhauMoi, CancellationToken cancellationToken = default);
    Task<ServiceResult> SendResetPasswordEmailAsync(string email, string linkTemplate, CancellationToken cancellationToken = default);
    Task<ServiceResult> ResetPasswordWithTokenAsync(string token, string matKhauMoi, CancellationToken cancellationToken = default);
}

public class AccountService(DbContext_TuiXach db, IPasswordService passwordService, IEmailService emailService, IMemoryCache cache) : IAccountService
{
    public async Task<TaiKhoan?> ValidateCredentialsAsync(string tenDangNhap, string matKhau, CancellationToken ct = default)
    {
        var account = await db.TaiKhoans.SingleOrDefaultAsync(x => x.tenDangNhap == tenDangNhap, ct);
        return account is not null && passwordService.Verify(account, matKhau) ? account : null;
    }

    public async Task<ServiceResult<TaiKhoan>> RegisterAsync(RegisterViewModel request, CancellationToken ct = default)
    {
        var tenDangNhap = request.TenDangNhap.Trim();
        var email = request.Email.Trim();

        if (await db.TaiKhoans.AnyAsync(x => x.tenDangNhap.ToLower() == tenDangNhap.ToLower(), ct))
        {
            return ServiceResult<TaiKhoan>.Fail("Tên đăng nhập này đã được sử dụng. Vui lòng chọn tên khác.", nameof(request.TenDangNhap));
        }

        if (await db.KhachHangs.AnyAsync(x => x.email != null && x.email.ToLower() == email.ToLower(), ct))
        {
            return ServiceResult<TaiKhoan>.Fail("Địa chỉ email này đã được đăng ký. Vui lòng chọn email khác hoặc đăng nhập.", nameof(request.Email));
        }

        // 3 bảng phải cùng thành công hoặc cùng thất bại -> transaction.
        // nếu có exception, await using sẽ dispose transaction và tự rollback.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        // vai trò luôn là khách hàng: tài khoản người bán chỉ được tạo bởi quản trị (seed/CSDL),
        // không bao giờ lấy vai trò từ dữ liệu form.
        var account = new TaiKhoan { tenDangNhap = tenDangNhap, vaiTro = Roles.Customer };
        account.matKhau = passwordService.Hash(account, request.MatKhau);
        db.TaiKhoans.Add(account);
        await db.SaveChangesAsync(ct);

        var customer = new KhachHang
        {
            maTK = account.maTK,
            hoTen = request.HoTen.Trim(),
            email = email,
            sdt = request.SoDienThoai.Trim(),
            diaChi = string.IsNullOrWhiteSpace(request.DiaChi) ? null : request.DiaChi.Trim()
        };
        db.KhachHangs.Add(customer);
        await db.SaveChangesAsync(ct);

        db.GioHangs.Add(new GioHang { maKH = customer.maKH, ngayTao = DateTime.Now });
        await db.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
        return ServiceResult<TaiKhoan>.Ok(account);
    }

    public async Task<ProfileViewModel?> GetProfileAsync(int maTk, CancellationToken ct = default)
    {
        var account = await db.TaiKhoans
            .Include(x => x.KhachHang)
                .ThenInclude(kh => kh!.DonHangs)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.maTK == maTk, ct);

        if (account is null) return null;

        var customer = account.KhachHang;
        var tongChiTieu = customer?.DonHangs.Where(d => d.trangThai != OrderStatus.DaHuy).Sum(d => d.tongTien) ?? 0;

        if(customer != null && !customer.isVip && tongChiTieu >= 5000000)
        {
            customer.isVip = true;
            await db.SaveChangesAsync(ct);    
        }

        return new ProfileViewModel
        {
            MaKH = customer?.maKH ?? 0,
            TenDangNhap = account.tenDangNhap,
            VaiTro = account.vaiTro,
            HoTen = customer?.hoTen ?? string.Empty,
            Email = customer?.email ?? string.Empty,
            SoDienThoai = customer?.sdt ?? string.Empty,
            DiaChi = customer?.diaChi,
            SoDonHang = customer?.DonHangs.Count ?? 0,
            TongChiTieu = customer?.DonHangs.Where(d => d.trangThai != OrderStatus.DaHuy).Sum(d => d.tongTien) ?? 0,
            IsVip = customer?.isVip ?? (tongChiTieu >= 5_000_000)   
        };
    }

    public async Task<ServiceResult> UpdateProfileAsync(int maTk, ProfileViewModel request, CancellationToken ct = default)
    {
        var account = await db.TaiKhoans.Include(x => x.KhachHang).FirstOrDefaultAsync(x => x.maTK == maTk, ct);
        if (account is null) return ServiceResult.Fail("Không tìm thấy tài khoản.");

        var email = request.Email.Trim();
        var emailExists = await db.KhachHangs.AnyAsync(
            x => x.maTK != maTk && x.email != null && x.email.ToLower() == email.ToLower(), ct);
        if (emailExists)
        {
            return ServiceResult.Fail("Địa chỉ email này đã được sử dụng bởi một tài khoản khác.", nameof(request.Email));
        }

        // tạo hồ sơ lần đầu khi người dùng tự gửi form.
        var customer = account.KhachHang;
        if (customer is null)
        {
            customer = new KhachHang { maTK = account.maTK };
            db.KhachHangs.Add(customer);
        }

        customer.hoTen = request.HoTen.Trim();
        customer.email = email;
        customer.sdt = request.SoDienThoai.Trim();
        customer.diaChi = string.IsNullOrWhiteSpace(request.DiaChi) ? null : request.DiaChi.Trim();

        await db.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ChangePasswordAsync(int maTk, string matKhauHienTai, string matKhauMoi, CancellationToken ct = default)
    {
        var account = await db.TaiKhoans.FirstOrDefaultAsync(x => x.maTK == maTk, ct);
        if (account is null) return ServiceResult.Fail("Không tìm thấy tài khoản.");

        if (!passwordService.Verify(account, matKhauHienTai))
        {
            return ServiceResult.Fail("Mật khẩu hiện tại không chính xác.");
        }

        account.matKhau = passwordService.Hash(account, matKhauMoi);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SendResetPasswordEmailAsync(string email, string linkTemplate, CancellationToken ct = default)
    {
        var cleanEmail = email.Trim().ToLower();
        var customer = await db.KhachHangs
            .FirstOrDefaultAsync(k => k.email != null && k.email.ToLower() == cleanEmail, ct);

        // Bảo mật: Dù có email hay không thì vẫn trả về OK để chống kẻ xấu dò email
        if (customer == null) return ServiceResult.Ok();

        // Tạo token ngẫu nhiên (32 ký tự hex) & lưu vào cache trong 15 phút
        var token = Guid.NewGuid().ToString("N");
        cache.Set($"{DoAnLTW.Constants.AppConstants.CacheKeys.PasswordResetPrefix}{token}", customer.maTK, TimeSpan.FromMinutes(15));

        // Ghép link và gửi email
        var resetLink = string.Format(linkTemplate, token);
        try
        {
            await emailService.SendPasswordResetAsync(customer.email!, customer.hoTen, resetLink, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email Error] Không gửi được mail reset password: {ex.Message}");
        }

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ResetPasswordWithTokenAsync(string token, string matKhauMoi, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token) || !cache.TryGetValue($"{DoAnLTW.Constants.AppConstants.CacheKeys.PasswordResetPrefix}{token}", out int maTk))
        {
            return ServiceResult.Fail("Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn (chỉ có hiệu lực trong 15 phút). Vui lòng yêu cầu lại.");
        }

        var account = await db.TaiKhoans.FindAsync([maTk], ct);
        if (account == null)
        {
            return ServiceResult.Fail("Tài khoản không tồn tại trong hệ thống.");
        }

        // Băm mật khẩu mới và lưu vào CSDL
        account.matKhau = passwordService.Hash(account, matKhauMoi);
        await db.SaveChangesAsync(ct);

        // Xóa token khỏi cache để không dùng lại được nữa
        cache.Remove($"pwd_reset_{token}");
        return ServiceResult.Ok();
    }
}
