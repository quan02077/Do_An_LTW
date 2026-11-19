using DoAnLTW.Areas.Admin.Services;
using DoAnLTW.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PromotionController(IPromotionService promotionService) : Controller
    {
        // 1. Danh sách khuyến mãi
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var promotions = await promotionService.GetAllAsync(cancellationToken);
            return View(promotions);
        }

        // 2. Thêm mới khuyến mãi
        public async Task<IActionResult> Add(CancellationToken cancellationToken)
        {
            var model = new PromotionFormViewModel
            {
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddMonths(1),
                TrangThai = true
            };
            await promotionService.PopulateProductOptionsAsync(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(PromotionFormViewModel model, CancellationToken cancellationToken)
        {
            if (model.NgayKetThuc < model.NgayBatDau)
            {
                ModelState.AddModelError(nameof(model.NgayKetThuc), "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            }

            if (!ModelState.IsValid)
            {
                // danh sách checkbox không được gửi lên từ form -> nạp lại, giữ nguyên các ô đã tick
                await promotionService.PopulateProductOptionsAsync(model, cancellationToken);
                return View(model);
            }

            var soSanPham = await promotionService.AddAsync(model, cancellationToken);
            TempData["Success"] = $"Đã thêm khuyến mãi '{model.TenKM}' cho {soSanPham} sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        // 3. Chỉnh sửa khuyến mãi
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var model = await promotionService.GetByIdAsync(id, cancellationToken);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PromotionFormViewModel model, CancellationToken cancellationToken)
        {
            if (model.NgayKetThuc < model.NgayBatDau)
            {
                ModelState.AddModelError(nameof(model.NgayKetThuc), "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            }

            if (!ModelState.IsValid)
            {
                await promotionService.PopulateProductOptionsAsync(model, cancellationToken);
                return View(model);
            }

            var soSanPham = await promotionService.UpdateAsync(id, model, cancellationToken);
            if (soSanPham == null) return NotFound();

            TempData["Success"] = $"Đã cập nhật khuyến mãi '{model.TenKM}' ({soSanPham} sản phẩm áp dụng).";
            return RedirectToAction(nameof(Index));
        }

        // 4. Xóa khuyến mãi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await promotionService.DeleteAsync(id, cancellationToken);
            if (!result)
            {
                TempData["Error"] = "Không thể xóa khuyến mãi này!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
