using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnLTW.Data;
using DoAnLTW.Models;
using DoAnLTW.Areas.Admin.ViewModels;

namespace DoAnLTW.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductsController : Controller
{
    private readonly DbContext_TuiXach _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(DbContext_TuiXach db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // get: /admin/products
    public async Task<IActionResult> Index(string? q, int? categoryId, int? brandId, int page = 1)
    {
        const int pageSize = 10;
        var query = _db.TuiXachs
            .Include(t => t.LoaiTui)
            .Include(t => t.ThuongHieu)
            .Include(t => t.HinhAnhTuis)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(t => t.tenTui.ToLower().Contains(search) || t.maTui.ToString().Contains(search));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(t => t.maLoai == categoryId.Value);
        }

        if (brandId.HasValue && brandId.Value > 0)
        {
            query = query.Where(t => t.maThuongHieu == brandId.Value);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

        var products = await query
            .OrderByDescending(t => t.maTui)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.SearchQuery = q;
        ViewBag.CategoryId = categoryId;
        ViewBag.BrandId = brandId;
        ViewBag.LoaiTuis = await _db.LoaiTuis.OrderBy(l => l.tenLoai).ToListAsync();
        ViewBag.ThuongHieus = await _db.ThuongHieus.OrderBy(th => th.tenThuongHieu).ToListAsync();

        return View(products);
    }

    // get: /admin/products/create
    public async Task<IActionResult> Create()
    {
        ViewBag.LoaiTuis = await _db.LoaiTuis.OrderBy(l => l.tenLoai).ToListAsync();
        ViewBag.ThuongHieus = await _db.ThuongHieus.OrderBy(th => th.tenThuongHieu).ToListAsync();
        return View(new ProductCreateViewModel());
    }

    // post: /admin/products/create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel model, IFormFile? hinhAnh)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.LoaiTuis = await _db.LoaiTuis.OrderBy(l => l.tenLoai).ToListAsync();
            ViewBag.ThuongHieus = await _db.ThuongHieus.OrderBy(th => th.tenThuongHieu).ToListAsync();
            return View(model);
        }

        var tuiXach = new TuiXach
        {
            tenTui = model.TenTui.Trim(),
            maLoai = model.MaLoai,
            maThuongHieu = model.MaThuongHieu,
            donGia = model.DonGia,
            soLuong = model.SoLuong,
            moTa = model.MoTa,
            badge = model.Badge
        };

        _db.TuiXachs.Add(tuiXach);
        await _db.SaveChangesAsync();

        // xu ly upload hinh anh
        string fileName = "tui1_1.png"; // anh mac dinh
        if (hinhAnh != null && hinhAnh.Length > 0)
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "HinhAnh");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var ext = Path.GetExtension(hinhAnh.FileName);
            fileName = $"tui_{tuiXach.maTui}_{DateTime.Now.Ticks}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await hinhAnh.CopyToAsync(stream);
            }
        }

        var anhTui = new HinhAnhTui
        {
            maTui = tuiXach.maTui,
            tenHinhAnh = fileName,
            laAnhChinh = true,
            thuTu = 1
        };
        _db.HinhAnhTuis.Add(anhTui);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã thêm sản phẩm '{tuiXach.tenTui}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    // get: /admin/products/edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var tui = await _db.TuiXachs
            .Include(t => t.HinhAnhTuis)
            .FirstOrDefaultAsync(t => t.maTui == id);

        if (tui == null)
        {
            TempData["Error"] = "Không tìm thấy sản phẩm cần sửa.";
            return RedirectToAction(nameof(Index));
        }

        var anhChinh = tui.HinhAnhTuis.FirstOrDefault(a => a.laAnhChinh)?.tenHinhAnh 
                      ?? tui.HinhAnhTuis.FirstOrDefault()?.tenHinhAnh;

        var model = new ProductEditViewModel
        {
            MaTui = tui.maTui,
            TenTui = tui.tenTui,
            MaLoai = tui.maLoai,
            MaThuongHieu = tui.maThuongHieu,
            DonGia = tui.donGia,
            SoLuong = tui.soLuong,
            MoTa = tui.moTa,
            Badge = tui.badge,
            AnhHienTai = anhChinh
        };

        ViewBag.LoaiTuis = await _db.LoaiTuis.OrderBy(l => l.tenLoai).ToListAsync();
        ViewBag.ThuongHieus = await _db.ThuongHieus.OrderBy(th => th.tenThuongHieu).ToListAsync();

        return View(model);
    }

    // post: /admin/products/edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductEditViewModel model, IFormFile? hinhAnhMoi)
    {
        if (id != model.MaTui)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.LoaiTuis = await _db.LoaiTuis.OrderBy(l => l.tenLoai).ToListAsync();
            ViewBag.ThuongHieus = await _db.ThuongHieus.OrderBy(th => th.tenThuongHieu).ToListAsync();
            return View(model);
        }

        var tui = await _db.TuiXachs
            .Include(t => t.HinhAnhTuis)
            .FirstOrDefaultAsync(t => t.maTui == id);

        if (tui == null)
        {
            TempData["Error"] = "Không tìm thấy sản phẩm để cập nhật.";
            return RedirectToAction(nameof(Index));
        }

        tui.tenTui = model.TenTui.Trim();
        tui.maLoai = model.MaLoai;
        tui.maThuongHieu = model.MaThuongHieu;
        tui.donGia = model.DonGia;
        tui.soLuong = model.SoLuong;
        tui.moTa = model.MoTa;
        tui.badge = model.Badge;

        // xu ly anh moi neu co upload
        if (hinhAnhMoi != null && hinhAnhMoi.Length > 0)
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "HinhAnh");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var ext = Path.GetExtension(hinhAnhMoi.FileName);
            var fileName = $"tui_{tui.maTui}_{DateTime.Now.Ticks}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await hinhAnhMoi.CopyToAsync(stream);
            }

            var anhChinh = tui.HinhAnhTuis.FirstOrDefault(a => a.laAnhChinh);
            if (anhChinh != null)
            {
                anhChinh.tenHinhAnh = fileName;
            }
            else
            {
                _db.HinhAnhTuis.Add(new HinhAnhTui
                {
                    maTui = tui.maTui,
                    tenHinhAnh = fileName,
                    laAnhChinh = true,
                    thuTu = 1
                });
            }
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã cập nhật sản phẩm '{tui.tenTui}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    // post: /admin/products/delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int maTui)
    {
        var tui = await _db.TuiXachs
            .Include(t => t.ChiTietDonHangs)
            .Include(t => t.ChiTietGioHangs)
            .Include(t => t.HinhAnhTuis)
            .FirstOrDefaultAsync(t => t.maTui == maTui);

        if (tui == null)
        {
            TempData["Error"] = "Không tìm thấy sản phẩm cần xóa.";
            return RedirectToAction(nameof(Index));
        }

        if (tui.ChiTietDonHangs.Any())
        {
            TempData["Error"] = $"Không thể xóa sản phẩm '{tui.tenTui}' vì đã có trong lịch sử đơn hàng của khách.";
            return RedirectToAction(nameof(Index));
        }

        _db.ChiTietGioHangs.RemoveRange(tui.ChiTietGioHangs);
        _db.HinhAnhTuis.RemoveRange(tui.HinhAnhTuis);
        _db.TuiXachs.Remove(tui);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã xóa sản phẩm '{tui.tenTui}' thành công.";
        return RedirectToAction(nameof(Index));
    }
}
