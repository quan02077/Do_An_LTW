using DoAnLTW.Areas.Admin.ViewModels;
using DoAnLTW.Data;
using DoAnLTW.Services;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Areas.Admin.Services
{
    public interface IAdminOrderService
    {
        Task<AdminOrderIndexViewModel> GetAllAsync(string? keyword = null, string? trangThai = null, int page = 1, CancellationToken cancellationToken = default);
        Task<AdminOrderFormViewModel?> GetByIdAsync(int maDh, CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateStatusAsync(int maDh, string trangThaiMoi, CancellationToken cancellationToken = default);
    }

    public class AdminOrderService(DbContext_TuiXach db) : IAdminOrderService
    {
        // 1. lay danh sach tat ca don hang co loc va tim kiem
        public async Task<AdminOrderIndexViewModel> GetAllAsync(string? keyword = null, string? trangThai = null, int page = 1, CancellationToken cancellationToken = default)
        {
            var query = db.DonHangs
                .AsNoTracking()
                .Include(d => d.KhachHang)
                .AsQueryable();

            // tim kiem theo ma don, ten khach hang hoac so dien thoai
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();

                // "DH07" hoặc "7" -> tìm đúng mã đơn 7. tách số ở phía C# vì EF Core không dịch được
                // chuỗi định dạng $"dh{maDH:D2}" sang SQL.
                int? maDhTim = int.TryParse(kw.StartsWith("dh") ? kw[2..] : kw, out var so) ? so : null;

                query = query.Where(d =>
                    (maDhTim != null && d.maDH == maDhTim) ||
                    (d.KhachHang != null && d.KhachHang.hoTen.ToLower().Contains(kw)) ||
                    (d.KhachHang != null && d.KhachHang.sdt != null && d.KhachHang.sdt.Contains(kw)));
            }

            // loc theo trang thai
            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "all")
            {
                query = query.Where(d => d.trangThai == trangThai);
            }

            // đếm tổng sau khi lọc rồi mới lấy đúng 10 dòng của trang hiện tại (Skip/Take chạy trong SQL)
            var total = await query.CountAsync(cancellationToken);
            page = PaginationViewModel.Normalize(page, total);
            var pageSize = PaginationViewModel.DefaultPageSize;

            // sap xep don moi nhat len dau (thêm maDH để thứ tự cố định khi trùng ngày đặt)
            var donHangs = await query
                .OrderByDescending(d => d.ngayDat)
                .ThenByDescending(d => d.maDH)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new AdminOrderViewModel
                {
                    MaDH = d.maDH,
                    MaKH = d.maKH,
                    TenKH = d.KhachHang != null ? d.KhachHang.hoTen : "Khách vãng lai",
                    Email = d.KhachHang != null && d.KhachHang.email != null ? d.KhachHang.email : "",
                    SDT = d.KhachHang != null && d.KhachHang.sdt != null ? d.KhachHang.sdt : "",
                    NgayDat = d.ngayDat,
                    TongTien = d.tongTien,
                    TrangThai = d.trangThai,
                    DiaChiNhan = d.diaChiNhan
                })
                .ToListAsync(cancellationToken);

            return new AdminOrderIndexViewModel
            {
                DonHangs = donHangs,
                Keyword = keyword,
                SelectedTrangThai = trangThai,
                TrangThaiOptions = OrderStatus.All.Select(s => new SelectListItem
                {
                    Value = s,
                    Text = s,
                    Selected = (s == trangThai)
                }).ToList(),
                Pagination = new PaginationViewModel
                {
                    Page = page,
                    TotalItems = total,
                    // giữ keyword + trạng thái khi chuyển trang
                    Filters = PaginationViewModel.BuildFilters(("keyword", keyword), ("trangThai", trangThai))
                }
            };
        }

        // 2. lay chi tiet mot don hang de xem va cap nhat trang thai
        public async Task<AdminOrderFormViewModel?> GetByIdAsync(int maDh, CancellationToken cancellationToken = default)
        {
            var donHang = await db.DonHangs
                .AsNoTracking()
                .Include(d => d.KhachHang)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.TuiXach)
                        .ThenInclude(t => t!.HinhAnhTuis)
                .FirstOrDefaultAsync(d => d.maDH == maDh, cancellationToken);

            if (donHang == null) return null;

            var items = donHang.ChiTietDonHangs.Select(ct => new AdminOrderItemViewModel
            {
                MaTui = ct.maTui,
                TenTui = ct.TuiXach != null ? ct.TuiXach.tenTui : "Túi xách",
                HinhAnh = ct.TuiXach?.HinhAnhTuis.OrderByDescending(h => h.laAnhChinh).ThenBy(h => h.thuTu).FirstOrDefault()?.tenHinhAnh,
                SoLuong = ct.soLuong,
                DonGia = ct.donGia
            }).ToList();

            return new AdminOrderFormViewModel
            {
                MaDH = donHang.maDH,
                MaKH = donHang.maKH,
                TenKH = donHang.KhachHang != null ? donHang.KhachHang.hoTen : "Khách vãng lai",
                Email = donHang.KhachHang != null && donHang.KhachHang.email != null ? donHang.KhachHang.email : "",
                SDT = donHang.KhachHang != null && donHang.KhachHang.sdt != null ? donHang.KhachHang.sdt : "",
                NgayDat = donHang.ngayDat,
                TongTien = donHang.tongTien,
                TrangThai = donHang.trangThai,
                DiaChiNhan = donHang.diaChiNhan,
                // chỉ đưa ra các trạng thái được phép chuyển tới từ trạng thái hiện tại
                TrangThaiOptions = OrderStatus.GetNextStatuses(donHang.trangThai)
                    .Select(s => new SelectListItem { Value = s, Text = s })
                    .ToList(),
                ChiTietDonHangs = items
            };
        }

        // 3. cap nhat trang thai don hang
        public async Task<ServiceResult> UpdateStatusAsync(int maDh, string trangThaiMoi, CancellationToken cancellationToken = default)
        {
            // cùng mức cô lập với OrderService của khách hàng: tránh admin và khách cùng hủy một đơn
            // rồi hoàn kho hai lần.
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var donHang = await db.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.TuiXach)
                .FirstOrDefaultAsync(d => d.maDH == maDh, cancellationToken);

            if (donHang == null) return ServiceResult.Fail("Không tìm thấy đơn hàng.");

            if (!OrderStatus.CanTransition(donHang.trangThai, trangThaiMoi))
            {
                var next = OrderStatus.GetNextStatuses(donHang.trangThai);
                return ServiceResult.Fail(next.Count == 0
                    ? $"Đơn hàng đang ở trạng thái '{donHang.trangThai}' nên không thể thay đổi nữa."
                    : $"Không thể chuyển từ '{donHang.trangThai}' sang '{trangThaiMoi}'. Chỉ được chuyển sang: {string.Join(", ", next)}.");
            }

            // chuyển sang 'Đã hủy' thì hoàn trả số lượng tồn kho
            if (trangThaiMoi == OrderStatus.DaHuy)
            {
                foreach (var item in donHang.ChiTietDonHangs)
                {
                    if (item.TuiXach != null)
                    {
                        item.TuiXach.soLuong += item.soLuong;
                    }
                }
            }

            donHang.trangThai = trangThaiMoi;
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ServiceResult.Ok();
        }
    }
}
