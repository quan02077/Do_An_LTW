using DoAnLTW.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers;
public class HomeController(IAdminDashboardService dashboardService) : AdminBaseController
{
    // get: /admin hoac /admin/home hoac /admin/home/index
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = await dashboardService.GetStatsAsync(ct);
        return View(model);
    }
}
