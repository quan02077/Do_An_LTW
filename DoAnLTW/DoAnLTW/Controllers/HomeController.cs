using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnLTW.Data;
using DoAnLTW.Models;
using DoAnLTW.Services;
using DoAnLTW.ViewModels;

namespace DoAnLTW.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbContext_TuiXach _db;
        private readonly IProductPricingService _pricingService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            DbContext_TuiXach db,
            IProductPricingService pricingService,
            ILogger<HomeController> logger)
        {
            _db = db;
            _pricingService = pricingService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var now = DateTime.Now;

            var products = await _db.TuiXachs
                .AsNoTracking()
                .Include(t => t.ThuongHieu)
                .Include(t => t.LoaiTui)
                .Include(t => t.KhuyenMai)
                .Include(t => t.HinhAnhTuis)
                .ToListAsync(ct);

            var productCards = products.Select(t =>
            {
                var priceInfo = _pricingService.GetPrice(t, now);
                var mainImg = t.HinhAnhTuis
                    .OrderByDescending(img => img.laAnhChinh)
                    .ThenBy(img => img.thuTu)
                    .FirstOrDefault()?.tenHinhAnh;

                var phanTramGiam = priceInfo.DonGiaGoc > 0 && priceInfo.SoTienGiam > 0
                    ? (double)Math.Round((priceInfo.SoTienGiam / priceInfo.DonGiaGoc) * 100, 0)
                    : 0;

                return new ProductCardItemViewModel(
                    MaTui: t.maTui,
                    TenTui: t.tenTui,
                    TenThuongHieu: t.ThuongHieu?.tenThuongHieu ?? "Basau",
                    TenLoai: t.LoaiTui?.tenLoai ?? "Túi xách",
                    DonGiaGoc: priceInfo.DonGiaGoc,
                    DonGiaHienTai: priceInfo.DonGiaApDung,
                    PhanTramGiam: phanTramGiam,
                    HinhAnh: mainImg,
                    SoLuongTonKho: t.soLuong,
                    Badge: t.badge
                );
            }).ToList();

            var brandsFromDb = await _db.ThuongHieus
                .AsNoTracking()
                .Include(th => th.TuiXachs)
                .ToListAsync(ct);

            var brands = brandsFromDb.Select(th => new BrandItemViewModel(
                th.maThuongHieu,
                th.tenThuongHieu,
                th.TuiXachs.Count,
                GetBrandSlogan(th.tenThuongHieu),
                GetBrandLogo(th.tenThuongHieu)
            )).ToList();

            var viewModel = new HomeIndexViewModel
            {
                HotProducts = productCards,
                Brands = brands
            };

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

        private static string GetBrandSlogan(string brandName) =>
            brandName.Trim().ToLowerInvariant() switch
            {
                "charles & keith" => "Quốc tế thanh lịch",
                "natoli" => "Balo công nghệ",
                "pedro" => "Hiện đại & Tinh xảo",
                "elly" => "Quý phái & Cổ điển",
                "jamlos" => "Canvas tối giản",
                _ => "Chính hãng cao cấp"
            };

        private static string? GetBrandLogo(string brandName) =>
            brandName.Trim().ToLowerInvariant() switch
            {
                "charles & keith" => "/HinhAnh/ck.jpg",
                "natoli" => "/HinhAnh/natoli.jpg",
                "pedro" => "/HinhAnh/pedro.jpg",
                "elly" => "/HinhAnh/elly.jpg",
                "jamlos" => "/HinhAnh/jamlos.jpg",
                _ => null
            };
    }
}
