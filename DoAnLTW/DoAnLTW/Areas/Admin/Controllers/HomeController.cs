using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers;

[Area("Admin")]
public class HomeController : Controller
{
    // GET: /Admin hoặc /Admin/Home hoặc /Admin/Home/Index
    public IActionResult Index()
    {
        return View();
    }
}
