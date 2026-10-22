using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers;

[Area("Admin")]
public class HomeController : Controller
{
    // get: /admin hoac /admin/home hoac /admin/home/index
    public IActionResult Index()
    {
        return View();
    }
}
