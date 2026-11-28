using DoAnLTW.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.ViewComponents;

/// <summary>
/// ViewComponent hiển thị widget số lượng giỏ hàng trên Header.
/// Tách rời logic đọc giỏ hàng khỏi Partial View để tuân thủ đúng kiến trúc MVC.
/// </summary>
public class CartSummaryViewComponent(ICartService cartService, ICurrentCustomerService currentCustomer) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        int count = 0;
        if (User.Identity?.IsAuthenticated == true)
        {
            var custId = await currentCustomer.GetCustomerIdAsync();
            if (custId.HasValue)
            {
                var cart = await cartService.GetAsync(custId.Value);
                count = cart.TongSoLuong;
            }
        }

        return View(count);
    }
}
