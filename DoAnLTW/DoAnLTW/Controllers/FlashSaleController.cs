using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

public class FlashSaleController : Controller
{
    // get: /flashsale hoặc /flashsale/index
    public IActionResult Index()
    {
        return View();
    }
}
