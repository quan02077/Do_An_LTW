using DoAnLTW.Areas.Admin.ViewModels;
using DoAnLTW.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var model = new DashboardViewModel
        {
            // tong so don hang
            TongDonHang = await _db.DonHangs.CountAsync(),

            // tong doanh thu tu cac don hang da hoan tat
            TongDoanhThu = await _db.DonHangs
                .Where(d => d.trangThai == "Hoàn tất")
                .SumAsync(d => (decimal?)d.tongTien) ?? 0,

            // so luong don hang theo tung trang thai
            DonChoDuyet = await _db.DonHangs.CountAsync(d => d.trangThai == "Chờ duyệt"),
            DonDangGiao = await _db.DonHangs.CountAsync(d => d.trangThai == "Đang giao"),
            DonHoanTat = await _db.DonHangs.CountAsync(d => d.trangThai == "Hoàn tất"),
            DonDaHuy = await _db.DonHangs.CountAsync(d => d.trangThai == "Đã hủy")
        };

        return View(model);
    }
}
