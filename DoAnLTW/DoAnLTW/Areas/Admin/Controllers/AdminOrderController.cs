using DoAnLTW.Areas.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminOrderController(IAdminOrderService orderService) : Controller
    {
        // 1. Danh sách đơn hàng
        public async Task<IActionResult> Index(string? keyword, string? trangThai, CancellationToken cancellationToken)
        {
            var model = await orderService.GetAllAsync(keyword, trangThai, cancellationToken);
            return View(model);
        }

        // 2. Chi tiết đơn hàng
        public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken)
        {
            var model = await orderService.GetByIdAsync(id, cancellationToken);
            if (model == null) return NotFound();
            return View(model);
        }

        // 3. Cập nhật trạng thái đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string trangThai, CancellationToken cancellationToken)
        {
            var success = await orderService.UpdateStatusAsync(id, trangThai, cancellationToken);
            if (!success)
            {
                TempData["Error"] = "Không thể cập nhật trạng thái đơn hàng.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            TempData["Success"] = $"Đã cập nhật trạng thái đơn hàng #DH{id:D2} thành '{trangThai}'.";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
