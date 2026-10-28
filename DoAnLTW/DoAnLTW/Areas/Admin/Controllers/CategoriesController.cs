using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnLTW.Data;
using DoAnLTW.Models;

namespace DoAnLTW.Areas.Admin.Controllers;

[Area("Admin")]
public class CategoriesController : Controller
{
    private readonly DbContext_TuiXach _db;

    public CategoriesController(DbContext_TuiXach db)
    {
        _db = db;
    }

    // get: /admin/categories
    public async Task<IActionResult> Index()
    {
        var loaiTuis = await _db.LoaiTuis
            .Include(lt => lt.TuiXachs)
            .OrderBy(lt => lt.maLoai)
            .ToListAsync();

        var thuongHieus = await _db.ThuongHieus
            .Include(th => th.TuiXachs)
            .OrderBy(th => th.maThuongHieu)
            .ToListAsync();

        ViewBag.LoaiTuis = loaiTuis;
        ViewBag.ThuongHieus = thuongHieus;

        return View();
    }

    // post: /admin/categories/createloaitui
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLoaiTui(string tenLoai)
    {
        if (string.IsNullOrWhiteSpace(tenLoai))
        {
            TempData["Error"] = "Tên loại túi không được để trống.";
            return RedirectToAction(nameof(Index));
        }

        var loaiTui = new LoaiTui { tenLoai = tenLoai.Trim() };
        _db.LoaiTuis.Add(loaiTui);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã thêm loại túi '{loaiTui.tenLoai}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    // post: /admin/categories/editloaitui
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLoaiTui(int maLoai, string tenLoai)
    {
        if (string.IsNullOrWhiteSpace(tenLoai))
        {
            TempData["Error"] = "Tên loại túi không được để trống.";
            return RedirectToAction(nameof(Index));
        }

        var loaiTui = await _db.LoaiTuis.FindAsync(maLoai);
        if (loaiTui == null)
        {
            TempData["Error"] = "Không tìm thấy loại túi cần sửa.";
            return RedirectToAction(nameof(Index));
        }

        loaiTui.tenLoai = tenLoai.Trim();
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã cập nhật loại túi '{loaiTui.tenLoai}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    // post: /admin/categories/deleteloaitui
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLoaiTui(int maLoai)
    {
        var loaiTui = await _db.LoaiTuis
            .Include(lt => lt.TuiXachs)
            .FirstOrDefaultAsync(lt => lt.maLoai == maLoai);

        if (loaiTui == null)
        {
            TempData["Error"] = "Không tìm thấy loại túi cần xóa.";
            return RedirectToAction(nameof(Index));
        }

        if (loaiTui.TuiXachs.Any())
        {
            TempData["Error"] = $"Không thể xóa loại túi '{loaiTui.tenLoai}' vì đang có {loaiTui.TuiXachs.Count} sản phẩm liên kết.";
            return RedirectToAction(nameof(Index));
        }

        _db.LoaiTuis.Remove(loaiTui);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã xóa loại túi '{loaiTui.tenLoai}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    // post: /admin/categories/createthuonghieu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateThuongHieu(string tenThuongHieu)
    {
        if (string.IsNullOrWhiteSpace(tenThuongHieu))
        {
            TempData["Error"] = "Tên thương hiệu không được để trống.";
            return RedirectToAction(nameof(Index));
        }

        var thuongHieu = new ThuongHieu { tenThuongHieu = tenThuongHieu.Trim() };
        _db.ThuongHieus.Add(thuongHieu);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã thêm thương hiệu '{thuongHieu.tenThuongHieu}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    // post: /admin/categories/editthuonghieu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditThuongHieu(int maThuongHieu, string tenThuongHieu)
    {
        if (string.IsNullOrWhiteSpace(tenThuongHieu))
        {
            TempData["Error"] = "Tên thương hiệu không được để trống.";
            return RedirectToAction(nameof(Index));
        }

        var thuongHieu = await _db.ThuongHieus.FindAsync(maThuongHieu);
        if (thuongHieu == null)
        {
            TempData["Error"] = "Không tìm thấy thương hiệu cần sửa.";
            return RedirectToAction(nameof(Index));
        }

        thuongHieu.tenThuongHieu = tenThuongHieu.Trim();
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã cập nhật thương hiệu '{thuongHieu.tenThuongHieu}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    // post: /admin/categories/deletethuonghieu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteThuongHieu(int maThuongHieu)
    {
        var thuongHieu = await _db.ThuongHieus
            .Include(th => th.TuiXachs)
            .FirstOrDefaultAsync(th => th.maThuongHieu == maThuongHieu);

        if (thuongHieu == null)
        {
            TempData["Error"] = "Không tìm thấy thương hiệu cần xóa.";
            return RedirectToAction(nameof(Index));
        }

        if (thuongHieu.TuiXachs.Any())
        {
            TempData["Error"] = $"Không thể xóa thương hiệu '{thuongHieu.tenThuongHieu}' vì đang có {thuongHieu.TuiXachs.Count} sản phẩm liên kết.";
            return RedirectToAction(nameof(Index));
        }

        _db.ThuongHieus.Remove(thuongHieu);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã xóa thương hiệu '{thuongHieu.tenThuongHieu}' thành công.";
        return RedirectToAction(nameof(Index));
    }
}
