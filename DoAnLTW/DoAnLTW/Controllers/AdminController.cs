using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoAnLTW.Models;

namespace DoAnLTW.Controllers;

[Authorize(Roles = Roles.Admin + ",Admin")]

public class AdminController : Controller
{
    // get: /admin hoặc /admin/index
    public IActionResult Index()
    {
        return View();
    }
}
