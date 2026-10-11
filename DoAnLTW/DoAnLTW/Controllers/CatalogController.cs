using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

public class CatalogController : Controller
{
    // GET: /Catalog hoặc /Catalog/Index
    public IActionResult Index(string? category, string? brand, string? q)
    {
        ViewData["Category"] = category;
        ViewData["Brand"] = brand;
        ViewData["SearchQuery"] = q;
        return View();
    }

    // GET: /Catalog/Detail?id=1
    public IActionResult Detail(int? id)
    {
        ViewData["ProductId"] = id ?? 1;
        return View();
    }
}
