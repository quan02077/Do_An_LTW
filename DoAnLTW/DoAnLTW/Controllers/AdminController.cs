using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

public class AdminController : Controller
{
    // GET: /Admin hoặc /Admin/Index
    public IActionResult Index()
    {
        return View();
    }
}
