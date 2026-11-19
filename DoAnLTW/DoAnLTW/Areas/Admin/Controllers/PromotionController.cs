using DoAnLTW.Areas.Admin.Services;
using DoAnLTW.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers
{
    public class PromotionController(IPromotionService promotionService) : AdminBaseController
    {
        // 1. Danh sách khuyến mãi
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var promotions = await promotionService.GetAllAsync(cancellationToken);
            return View(promotions);
        }

        // 2. Thêm mới khuyến mãi
        public IActionResult Add()
        {
            var model = new PromotionFormViewModel
            {
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddMonths(1),
                TrangThai = true
            };
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
                return View(model);
            }

            await promotionService.AddAsync(model, cancellationToken);
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
                return View(model);
            }

            var result = await promotionService.UpdateAsync(id, model, cancellationToken);
            if (!result) return NotFound();

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
