using DoAnLTW.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

public class CatalogController(ICatalogService catalogService) : Controller
{
    // 1. Danh sach san pham & bo loc
    public async Task<IActionResult> Index(
        string? category,
        string? brand,
        string? q,
        string? priceRange,
        string? sort,
        CancellationToken ct)
    {
        var viewModel = await catalogService.GetCatalogAsync(category, brand, q, priceRange, sort, ct);
        return View(viewModel);
    }

    // 2. Chi tiet san pham
    public async Task<IActionResult> Detail(int? id, CancellationToken ct)
    {
        if (id == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var viewModel = await catalogService.GetDetailAsync(id.Value, ct);
        if (viewModel == null)
        {
            return NotFound();
        }

        return View(viewModel);
    }
}
