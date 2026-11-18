using DoAnLTW.Areas.Admin.Services;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController(IProductService productService) : Controller
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
            await productService.PopulateFormOptionsAsync(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(ProductFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await productService.PopulateFormOptionsAsync(model, cancellationToken);
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

            await productService.PopulateFormOptionsAsync(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await productService.PopulateFormOptionsAsync(model, cancellationToken);
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
    }
}
