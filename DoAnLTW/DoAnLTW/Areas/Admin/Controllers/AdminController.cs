using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminController : Controller
{
    // Chuyển hướng các request cũ về HomeController của Area Admin
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Home", new { area = "Admin" });
    }
}

