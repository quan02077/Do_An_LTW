using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

public class CatalogController : Controller
{
    public IActionResult Index(string? category, string? brand, string? q)
    {
        ViewData["Category"] = category;
        ViewData["Brand"] = brand;
        ViewData["SearchQuery"] = q;
        return View();
    }

    public IActionResult Detail(int? id)
    {
        ViewData["ProductId"] = id ?? 1;
        return View();
    }
}
