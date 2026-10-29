using DoAnLTW.Areas.Admin.Services;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController(ICategoryService categoryService) : Controller
    {
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = await categoryService.GetAllAsync(cancellationToken);
            return View(model);
        }

        public IActionResult AddLoaiTui()
        {
            return View();
        }

        public IActionResult AddThuongHieu()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddLoaiTui(TypeCreateViewModel loaiTui, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(loaiTui);
            }
            await categoryService.AddLoaiTuiAsync(loaiTui, cancellationToken);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddThuongHieu(BrandCreateViewModel thuongHieu, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(thuongHieu);
            }
            await categoryService.AddThuongHieuAsync(thuongHieu, cancellationToken);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditLoaiTui(int maLoai, CancellationToken cancellationToken)
        {
            var loaiTui = await categoryService.GetLoaiTuiByIdAsync(maLoai, cancellationToken);
            if (loaiTui == null)
            {
                return NotFound();
            }
            return View(loaiTui);
        }

        public async Task<IActionResult> EditThuongHieu(int maThuongHieu, CancellationToken cancellationToken)
        {
            var thuongHieu = await categoryService.GetThuongHieuByIdAsync(maThuongHieu, cancellationToken);
            if (thuongHieu == null)
            {
                return NotFound();
            }
            return View(thuongHieu);
        }

        [HttpPost]
        public async Task<IActionResult> EditLoaiTui(int maLoai, TypeEditViewModel loaiTui, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(loaiTui);
            }
            await categoryService.UpdateLoaiTuiAsync(maLoai, loaiTui, cancellationToken);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditThuongHieu(int maThuongHieu, BrandEditViewModel thuongHieu, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(thuongHieu);
            }
            await categoryService.UpdateThuongHieuAsync(maThuongHieu, thuongHieu, cancellationToken);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLoaiTui(int maLoai, CancellationToken cancellationToken)
        {
            var result = await categoryService.DeleteLoaiTuiAsync(maLoai, cancellationToken);
            if (!result)
            {
                TempData["Error"] = "Không thể xóa loại túi này vì đang có sản phẩm thuộc về nó!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteThuongHieu(int maThuongHieu, CancellationToken cancellationToken)
        {
            var result = await categoryService.DeleteThuongHieuAsync(maThuongHieu, cancellationToken);
            if (!result)
            {
                TempData["Error"] = "Không thể xóa thương hiệu này vì đang có sản phẩm thuộc về nó!";
            }
            return RedirectToAction("Index");
        }
    }
}