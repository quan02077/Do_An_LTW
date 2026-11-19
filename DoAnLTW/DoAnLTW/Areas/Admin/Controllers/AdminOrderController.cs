using DoAnLTW.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers
{
    public class AdminOrderController(IAdminOrderService orderService) : AdminBaseController
    {
        // 1. Danh sách đơn hàng
        // GET: /Admin/AdminOrder?keyword=...&trangThai=...&page=2
        public async Task<IActionResult> Index(string? keyword, string? trangThai, int page = 1, CancellationToken cancellationToken = default)
        {
            var model = await orderService.GetAllAsync(keyword, trangThai, page, cancellationToken);
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
            var result = await orderService.UpdateStatusAsync(id, trangThai ?? string.Empty, cancellationToken);
            if (!result.Success)
            {
                // hiển thị lý do cụ thể (ví dụ: đơn đã hủy thì không giao lại được)
                TempData["Error"] = result.Error;
                return RedirectToAction(nameof(Detail), new { id });
            }

            TempData["Success"] = $"Đã cập nhật trạng thái đơn hàng #DH{id:D2} thành '{trangThai}'.";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
