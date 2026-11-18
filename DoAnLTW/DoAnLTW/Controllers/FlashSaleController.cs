using DoAnLTW.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

public class FlashSaleController(IFlashSaleService flashSaleService) : Controller
{
    // get: /flashsale hoac /flashsale/index
    public async Task<IActionResult> Index(int? kmId, CancellationToken ct)
    {
        var viewModel = await flashSaleService.GetFlashSaleAsync(kmId, ct);
        return View(viewModel);
    }
}
