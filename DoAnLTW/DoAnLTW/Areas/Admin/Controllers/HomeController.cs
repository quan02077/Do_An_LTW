using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnLTW.Data;
using DoAnLTW.Areas.Admin.ViewModels;

namespace DoAnLTW.Areas.Admin.Controllers;

[Area("Admin")]
public class HomeController : Controller
{
    private readonly DbContext_TuiXach _db;

    public HomeController(DbContext_TuiXach db)
    {
        _db = db;
    }

    // get: /admin hoac /admin/home hoac /admin/home/index
    public async Task<IActionResult> Index()
    {
        var totalOrders = await _db.DonHangs.CountAsync();
        var deliveredOrders = await _db.DonHangs.CountAsync(d => d.trangThai == "Đã giao");
        var pendingOrders = await _db.DonHangs.CountAsync(d => d.trangThai == "Chờ xử lý" || d.trangThai == "Đang giao");
        var canceledOrders = await _db.DonHangs.CountAsync(d => d.trangThai == "Đã hủy");

        var totalRevenue = await _db.DonHangs
            .Where(d => d.trangThai == "Đã giao")
            .SumAsync(d => (decimal?)d.tongTien) ?? 0;

        var pendingAmount = await _db.DonHangs
            .Where(d => d.trangThai == "Chờ xử lý" || d.trangThai == "Đang giao")
            .SumAsync(d => (decimal?)d.tongTien) ?? 0;

        var canceledAmount = await _db.DonHangs
            .Where(d => d.trangThai == "Đã hủy")
            .SumAsync(d => (decimal?)d.tongTien) ?? 0;

        var totalProducts = await _db.TuiXachs.CountAsync();
        var totalCategories = await _db.LoaiTuis.CountAsync();
        var totalBrands = await _db.ThuongHieus.CountAsync();

        var recentOrders = await _db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTietDonHangs)
            .OrderByDescending(d => d.ngayDat)
            .Take(8)
            .ToListAsync();

        var vm = new AdminDashboardViewModel
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            DeliveredOrders = deliveredOrders,
            DeliveredAmount = totalRevenue,
            PendingOrders = pendingOrders,
            PendingAmount = pendingAmount,
            CanceledOrders = canceledOrders,
            CanceledAmount = canceledAmount,
            TotalProducts = totalProducts,
            TotalCategories = totalCategories,
            TotalBrands = totalBrands,
            RecentOrders = recentOrders
        };

        return View(vm);
    }
}
