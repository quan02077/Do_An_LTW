using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DoAnLTW.Models;
using DoAnLTW.Services;

namespace DoAnLTW.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeRender _homeRender;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IHomeRender homeRender,
            ILogger<HomeController> logger)
        {
            _homeRender = homeRender;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var viewModel = await _homeRender.GetAsync(ct);
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
