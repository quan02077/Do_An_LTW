using DoAnLTW.Areas.Admin.Services;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers
{
    public class ProductController(IProductService productService) : AdminBaseController
    {
        // 1. Danh sách sản phẩm
        // GET: /Admin/Product?keyword=...
        public async Task<IActionResult> Index(string? keyword, CancellationToken cancellationToken)
        {
            var model = await productService.GetAllAsync(keyword, cancellationToken);
            return View(model);
        }

        // 2. Thêm mới sản phẩm
        public async Task<IActionResult> Add(CancellationToken cancellationToken)
        {
            var model = new ProductFormViewModel();
            await productService.PopulateFormOptionsAsync(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProductFormViewModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var result = await productService.AddAsync(model, cancellationToken);
                if (result.Success)
                {
                    TempData["Success"] = "Đã thêm sản phẩm.";
                    return RedirectToAction("Index");
                }

                // lỗi ảnh (sai đuôi/quá 2MB) hiển thị ngay dưới ô chọn file, không thành lỗi 500.
                ModelState.AddModelError(result.Field ?? string.Empty, result.Error!);
            }

            await productService.PopulateFormOptionsAsync(model, cancellationToken);
            return View(model);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model, CancellationToken cancellationToken)
        {
            var stockConflict = false;
            if (ModelState.IsValid)
            {
                var result = await productService.UpdateAsync(id, model, cancellationToken);
                if (result.Success)
                {
                    TempData["Success"] = "Đã cập nhật sản phẩm.";
                    return RedirectToAction("Index");
                }

                stockConflict = result.Field == nameof(model.SoLuong);
                ModelState.AddModelError(result.Field ?? string.Empty, result.Error!);
            }

            // hiển thị lại form: lấy lại ảnh hiện tại từ CSDL vì form không gửi trường này lên.
            var current = await productService.GetByIdAsync(id, cancellationToken);
            if (current == null) return NotFound();

            model.MaTui = id;
            model.HinhAnhHienTai = current.HinhAnhHienTai;

            if (stockConflict)
            {
                // cập nhật "tồn kho gốc" thành số hiện tại để lần lưu tiếp theo (admin đã thấy số mới) được chấp nhận.
                // phải xóa giá trị cũ trong ModelState, nếu không trường ẩn vẫn hiển thị số cũ do model binding.
                ModelState.Remove(nameof(model.SoLuongGoc));
                model.SoLuongGoc = current.SoLuongGoc;
            }
            await productService.PopulateFormOptionsAsync(model, cancellationToken);
            return View(model);
        }

        // 4. Xóa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await productService.DeleteAsync(id, cancellationToken);
            if (result.Success)
            {
                TempData["Success"] = "Đã xóa sản phẩm.";
            }
            else
            {
                TempData["Error"] = result.Error;
            }
            return RedirectToAction("Index");
        }
    }
}
