using DoAnLTW.Areas.Admin.ViewModels;
using DoAnLTW.Data;
using DoAnLTW.Services;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Areas.Admin.Services;

public interface IAdminDashboardService
{
    Task<DashboardViewModel> GetStatsAsync(int? year, CancellationToken ct = default);
}

public class AdminDashboardService(DbContext_TuiXach db) : IAdminDashboardService
{
    public async Task<DashboardViewModel> GetStatsAsync(int? year, CancellationToken ct = default)
    {
        var currentYear = DateTime.Now.Year;
        var years = await db.DonHangs.Select(d => d.ngayDat.Year).Distinct().ToListAsync(ct);
        years.Add(currentYear);
        years = years.Distinct().OrderByDescending(y => y).ToList();
        var selectedYear = year.HasValue && years.Contains(year.Value) ? year.Value : currentYear;

        var monthlyRevenue = await db.DonHangs
            .Where(d => d.trangThai == OrderStatus.HoanTat && d.ngayDat.Year == selectedYear)
            .GroupBy(d => d.ngayDat.Month)
            .Select(g => new { Month = g.Key, Revenue = g.Sum(d => d.tongTien) })
            .ToListAsync(ct);

        var topProducts = await db.ChiTietDonHangs
            .Where(ctdh => ctdh.DonHang!.trangThai == OrderStatus.HoanTat)
            .GroupBy(ctdh => new { ctdh.maTui, ctdh.TuiXach!.tenTui })
            .Select(g => new SanPhamBanChayViewModel
            {
                TenTui = g.Key.tenTui,
                SoLuongDaBan = g.Sum(ctdh => ctdh.soLuong),
                DoanhThu = g.Sum(ctdh => ctdh.thanhTien)
            })
            .OrderByDescending(p => p.SoLuongDaBan)
            .ThenBy(p => p.TenTui)
            .Take(5)
            .ToListAsync(ct);

        var model = new DashboardViewModel
        {
            NamDuocChon = selectedYear,
            DanhSachNam = years,
            SanPhamBanChay = topProducts,
            TongDonHang = await db.DonHangs.CountAsync(ct),
            TongDoanhThu = await db.DonHangs
                .Where(d => d.trangThai == OrderStatus.HoanTat)
                .SumAsync(d => (decimal?)d.tongTien, ct) ?? 0,
            DonChoDuyet = await db.DonHangs.CountAsync(d => d.trangThai == OrderStatus.ChoDuyet, ct),
            DonDangGiao = await db.DonHangs.CountAsync(d => d.trangThai == OrderStatus.DangGiao, ct),
            DonHoanTat = await db.DonHangs.CountAsync(d => d.trangThai == OrderStatus.HoanTat, ct),
            DonDaHuy = await db.DonHangs.CountAsync(d => d.trangThai == OrderStatus.DaHuy, ct)
        };
        // Bổ sung các tháng không có đơn hoàn tất bằng doanh thu 0.
        foreach (var month in monthlyRevenue)
            model.DoanhThuTheoThang[month.Month - 1] = month.Revenue;

        return model;
    }
}
