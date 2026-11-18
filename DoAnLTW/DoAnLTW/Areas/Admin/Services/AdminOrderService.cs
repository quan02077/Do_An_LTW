using DoAnLTW.Areas.Admin.ViewModels;
using DoAnLTW.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Areas.Admin.Services
{
    public interface IAdminOrderService
    {
        Task<AdminOrderIndexViewModel> GetAllAsync(string? keyword = null, string? trangThai = null, CancellationToken cancellationToken = default);
        Task<AdminOrderFormViewModel?> GetByIdAsync(int maDh, CancellationToken cancellationToken = default);
        Task<bool> UpdateStatusAsync(int maDh, string trangThaiMoi, CancellationToken cancellationToken = default);
    }

    public class AdminOrderService(DbContext_TuiXach db) : IAdminOrderService
    {
        // 4 trang thai chuan cua don hang
        private static readonly List<string> StatusList = new()
        {
            "Chờ duyệt",
            "Đang giao",
            "Hoàn tất",
            "Đã hủy"
        };

        // 1. lay danh sach tat ca don hang co loc va tim kiem
        public async Task<AdminOrderIndexViewModel> GetAllAsync(string? keyword = null, string? trangThai = null, CancellationToken cancellationToken = default)
        {
            var query = db.DonHangs
                .AsNoTracking()
                .Include(d => d.KhachHang)
                .AsQueryable();

            // tim kiem theo ma don, ten khach hang hoac so dien thoai
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(d =>
                    d.maDH.ToString().Contains(kw) ||
                    ($"dh{d.maDH:D2}").ToLower().Contains(kw) ||
                    (d.KhachHang != null && d.KhachHang.hoTen.ToLower().Contains(kw)) ||
                    (d.KhachHang != null && d.KhachHang.sdt != null && d.KhachHang.sdt.Contains(kw)));
            }

            // loc theo trang thai
            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "all")
            {
                query = query.Where(d => d.trangThai == trangThai);
            }

            // sap xep don moi nhat len dau
            var donHangs = await query
                .OrderByDescending(d => d.ngayDat)
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
                TrangThaiOptions = StatusList.Select(s => new SelectListItem
                {
                    Value = s,
                    Text = s,
                    Selected = (s == trangThai)
                }).ToList()
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
                TrangThaiOptions = StatusList.Select(s => new SelectListItem
                {
                    Value = s,
                    Text = s,
                    Selected = (s == donHang.trangThai)
                }).ToList(),
                ChiTietDonHangs = items
            };
        }

        // 3. cap nhat trang thai don hang
        public async Task<bool> UpdateStatusAsync(int maDh, string trangThaiMoi, CancellationToken cancellationToken = default)
        {
            var donHang = await db.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.TuiXach)
                .FirstOrDefaultAsync(d => d.maDH == maDh, cancellationToken);

            if (donHang == null) return false;

            // neu chuyen sang 'Da huy' thi hoan tra lai so luong ton kho
            if (trangThaiMoi == "Đã hủy" && donHang.trangThai != "Đã hủy")
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
            return true;
        }
    }
}
