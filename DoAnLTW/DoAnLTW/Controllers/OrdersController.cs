using DoAnLTW.Services;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

[Authorize]
public class OrdersController(
    ICartService cartService,
    IOrderService orderService,
    ICurrentCustomerService currentCustomer) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Checkout(CancellationToken ct)
    {
        var maKh = await currentCustomer.GetRequiredCustomerIdAsync(ct);
        var cart = await cartService.GetAsync(maKh, ct);
        if (cart.Items.Count == 0)
        {
            TempData["CartError"] = "Giỏ hàng đang trống.";
            return RedirectToAction("Index", "Cart");
        }

        return View(new CheckoutPageViewModel(cart, new CheckoutViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout([Bind(Prefix = "Checkout")] CheckoutViewModel request, CancellationToken ct)
    {
        var maKh = await currentCustomer.GetRequiredCustomerIdAsync(ct);
        if (!ModelState.IsValid)
        {
            return View(new CheckoutPageViewModel(await cartService.GetAsync(maKh, ct), request));
        }

        try
        {
            var order = await orderService.CheckoutAsync(maKh, request, ct);
            TempData["OrderSuccess"] = $"Đã tạo đơn hàng #{order.MaDonHang.ToString()[..8]}.";
            return RedirectToAction(nameof(History));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(new CheckoutPageViewModel(await cartService.GetAsync(maKh, ct), request));
        }
    }

    [HttpGet]
    public async Task<IActionResult> History(CancellationToken ct) =>
        View(await orderService.GetHistoryAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid maDh, CancellationToken ct)
    {
        try
        {
            await orderService.CancelAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), maDh, ct);
            TempData["OrderSuccess"] = "Đã hủy đơn hàng và hoàn lại tồn kho.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["OrderError"] = ex.Message;
        }

        return RedirectToAction(nameof(History));
    }
}
