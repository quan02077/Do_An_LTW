using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoAnLTW.Models;

namespace DoAnLTW.Controllers;

[Authorize(Roles = Roles.Admin + ",Admin")]

public class AdminController : Controller
{
    // GET: /Admin hoặc /Admin/Index
    public IActionResult Index()
    {
        return View();
    }
}
