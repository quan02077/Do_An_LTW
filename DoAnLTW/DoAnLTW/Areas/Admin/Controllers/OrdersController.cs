using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnLTW.Data;
using DoAnLTW.Models;

namespace DoAnLTW.Areas.Admin.Controllers;

[Area("Admin")]
public class OrdersController : Controller
{
    private readonly DbContext_TuiXach _db;

    public OrdersController(DbContext_TuiXach db)
    {
        _db = db;
    }

    // get: /admin/orders
    public async Task<IActionResult> Index(string? status, string? q)
    {
        var query = _db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTietDonHangs)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            query = query.Where(d => d.trangThai == status);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(d => d.maDH.ToString().Contains(search) 
                                  || d.KhachHang!.hoTen.ToLower().Contains(search)
                                  || d.diaChiNhan.ToLower().Contains(search));
        }

        var orders = await query
            .OrderByDescending(d => d.ngayDat)
            .ToListAsync();

        ViewBag.CurrentStatus = status ?? "All";
        ViewBag.SearchQuery = q;

        return View(orders);
    }

    // get: /admin/orders/details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.TuiXach)
                    .ThenInclude(t => t!.HinhAnhTuis)
            .FirstOrDefaultAsync(d => d.maDH == id);

        if (order == null)
        {
            TempData["Error"] = "Không tìm thấy đơn hàng.";
            return RedirectToAction(nameof(Index));
        }

        return View(order);
    }

    // post: /admin/orders/updatestatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int maDH, string trangThai)
    {
        var order = await _db.DonHangs.FindAsync(maDH);
        if (order == null)
        {
            TempData["Error"] = "Không tìm thấy đơn hàng cần cập nhật.";
            return RedirectToAction(nameof(Index));
        }

        order.trangThai = trangThai;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã cập nhật trạng thái đơn hàng DH{order.maDH:D2} sang '{trangThai}'.";
        return RedirectToAction(nameof(Details), new { id = maDH });
    }
}
