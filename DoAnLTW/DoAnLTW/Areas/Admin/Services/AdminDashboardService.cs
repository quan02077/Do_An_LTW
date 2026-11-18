using DoAnLTW.Areas.Admin.ViewModels;
using DoAnLTW.Data;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Areas.Admin.Services;

public interface IAdminDashboardService
{
    Task<DashboardViewModel> GetStatsAsync(CancellationToken ct = default);
}

public class AdminDashboardService(DbContext_TuiXach db) : IAdminDashboardService
{
    public async Task<DashboardViewModel> GetStatsAsync(CancellationToken ct = default)
    {
        return new DashboardViewModel
        {
            TongDonHang = await db.DonHangs.CountAsync(ct),
            TongDoanhThu = await db.DonHangs
                .Where(d => d.trangThai == "Hoàn tất")
                .SumAsync(d => (decimal?)d.tongTien, ct) ?? 0,
            DonChoDuyet = await db.DonHangs.CountAsync(d => d.trangThai == "Chờ duyệt", ct),
            DonDangGiao = await db.DonHangs.CountAsync(d => d.trangThai == "Đang giao", ct),
            DonHoanTat = await db.DonHangs.CountAsync(d => d.trangThai == "Hoàn tất", ct),
            DonDaHuy = await db.DonHangs.CountAsync(d => d.trangThai == "Đã hủy", ct)
        };
    }
}
