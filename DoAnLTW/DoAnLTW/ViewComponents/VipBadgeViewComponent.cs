using DoAnLTW.Data;
using DoAnLTW.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.ViewComponents;

/// <summary>
/// ViewComponent hiển thị huy hiệu VIP bên cạnh tên người dùng trên Header.
/// Tự động kiểm tra quyền và hạng thành viên một cách độc lập.
/// </summary>
public class VipBadgeViewComponent(ICurrentCustomerService currentCustomer, DbContext_TuiXach db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Content(string.Empty);
        }

        var custId = await currentCustomer.GetCustomerIdAsync();
        if (!custId.HasValue)
        {
            return Content(string.Empty);
        }

        var isVip = await db.KhachHangs.AsNoTracking()
            .Where(k => k.maKH == custId.Value)
            .Select(k => k.isVip)
            .SingleOrDefaultAsync();

        return View(isVip);
    }
}
