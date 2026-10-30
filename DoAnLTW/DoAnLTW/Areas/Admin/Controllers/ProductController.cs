using DoAnLTW.Areas.Admin.Services;
using DoAnLTW.Data;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController(IProductService productService, DbContext_TuiXach db) : Controller
    {
        // 1. Danh sách sản phẩm
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var products = await productService.GetAllAsync(cancellationToken);
            return View(products);
        }

        // 2. Thêm mới sản phẩm
        public async Task<IActionResult> Add(CancellationToken cancellationToken)
        {
            var model = new ProductFormViewModel();
            await PopulateDropdowns(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(ProductFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model, cancellationToken);
                return View(model);
            }

            await productService.AddAsync(model, cancellationToken);
            return RedirectToAction("Index");
        }

        // 3. Chỉnh sửa sản phẩm
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var model = await productService.GetByIdAsync(id, cancellationToken);
            if (model == null) return NotFound();

            await PopulateDropdowns(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model, cancellationToken);
                return View(model);
            }

            await productService.UpdateAsync(id, model, cancellationToken);
            return RedirectToAction("Index");
        }

        // 4. Xóa sản phẩm
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await productService.DeleteAsync(id, cancellationToken);
            if (!result)
            {
                TempData["Error"] = "Không thể xóa sản phẩm này!";
            }
            return RedirectToAction("Index");
        }

        // nạp dữ liệu cho 2 dropdown Loại túi và Thương hiệu
        private async Task PopulateDropdowns(ProductFormViewModel model, CancellationToken cancellationToken)
        {
            model.LoaiTuiOptions = await db.LoaiTuis
                .Select(lt => new SelectListItem { Value = lt.maLoai.ToString(), Text = lt.tenLoai })
                .ToListAsync(cancellationToken);

            model.ThuongHieuOptions = await db.ThuongHieus
                .Select(th => new SelectListItem { Value = th.maThuongHieu.ToString(), Text = th.tenThuongHieu })
                .ToListAsync(cancellationToken);
        }
    }
}

