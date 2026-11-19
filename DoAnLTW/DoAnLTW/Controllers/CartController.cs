using DoAnLTW.Services;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

[Authorize]
public class CartController(ICartService cartService, ICurrentCustomerService currentCustomer) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await cartService.GetAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddCartItemViewModel request, string? returnUrl, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["CartError"] = "Số lượng sản phẩm không hợp lệ.";
            return LocalRedirect(returnUrl ?? Url.Action(nameof(Index))!);
        }

        try
        {
            await cartService.AddAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), request, ct);
            TempData["CartSuccess"] = "Đã thêm sản phẩm vào giỏ hàng.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["CartError"] = ex.Message;
        }

        return LocalRedirect(returnUrl ?? Url.Action(nameof(Index))!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int maTui, UpdateCartItemViewModel request, CancellationToken ct)
    {
        // [Range(1, 99)] chỉ ghi lỗi vào ModelState, không tự chặn request:
        // controller MVC phải tự kiểm tra trước khi gọi service.
        if (!ModelState.IsValid)
        {
            TempData["CartError"] = "Số lượng phải từ 1 đến 99. Muốn bỏ sản phẩm, hãy bấm Xóa.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await cartService.UpdateAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), maTui, request, ct);
            TempData["CartSuccess"] = "Đã cập nhật giỏ hàng.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["CartError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int maTui, CancellationToken ct)
    {
        await cartService.RemoveAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), maTui, ct);
        TempData["CartSuccess"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }
}
