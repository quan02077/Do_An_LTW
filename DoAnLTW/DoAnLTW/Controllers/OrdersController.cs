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
    // GET: /Orders/Checkout?selected=1&selected=4 (danh sách sản phẩm khách tick chọn trong giỏ)
    public async Task<IActionResult> Checkout(List<int>? selected, CancellationToken ct)
    {
        var maKh = await currentCustomer.GetRequiredCustomerIdAsync(ct);
        var cart = await cartService.GetAsync(maKh, ct);
        if (cart.Items.Count == 0)
        {
            TempData["CartError"] = "Giỏ hàng đang trống.";
            return RedirectToAction("Index", "Cart");
        }

        // chỉ giữ mã túi thật sự có trong giỏ (bỏ qua mã lạ do sửa URL)
        var selectedCart = cart.Only(selected ?? []);
        if (selectedCart.Items.Count == 0)
        {
            TempData["CartError"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.";
            return RedirectToAction("Index", "Cart");
        }

        var form = await orderService.GetCheckoutDefaultsAsync(maKh, selectedCart.Items.Select(x => x.MaTui), ct);
        return View(new CheckoutPageViewModel(selectedCart, form));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout([Bind(Prefix = "Checkout")] CheckoutViewModel request, CancellationToken ct)
    {
        var maKh = await currentCustomer.GetRequiredCustomerIdAsync(ct);
        if (!ModelState.IsValid)
        {
            return await CheckoutViewAsync(maKh, request, ct);
        }

        try
        {
            var order = await orderService.CheckoutAsync(maKh, request, ct);
            TempData["OrderSuccess"] = $"Đã tạo đơn hàng #{order.MaHienThi}.";
            return RedirectToAction(nameof(History));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await CheckoutViewAsync(maKh, request, ct);
        }
    }

    // hiển thị lại trang thanh toán với đúng các sản phẩm đã chọn và dữ liệu khách vừa nhập
    private async Task<IActionResult> CheckoutViewAsync(int maKh, CheckoutViewModel request, CancellationToken ct)
    {
        var cart = await cartService.GetAsync(maKh, ct);
        return View(nameof(Checkout), new CheckoutPageViewModel(cart.Only(request.SelectedMaTui), request));
    }

    [HttpGet]
    public async Task<IActionResult> History(CancellationToken ct) =>
        View(await orderService.GetHistoryAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int maDh, CancellationToken ct)
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
