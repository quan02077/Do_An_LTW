using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

public class FlashSaleController : Controller
{
    // GET: /FlashSale hoặc /FlashSale/Index
    public IActionResult Index()
    {
        return View();
    }
}
