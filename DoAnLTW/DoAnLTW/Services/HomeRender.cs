using DoAnLTW.Data;
using DoAnLTW.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Services;

public interface IHomeRender
{
    Task<HomeIndexViewModel> GetAsync(CancellationToken ct = default);
}

public class HomeRender : IHomeRender
{
    private readonly DbContext_TuiXach _db;
    private readonly IProductPricingService _pricingService;

    public HomeRender(
        DbContext_TuiXach db,
        IProductPricingService pricingService)
    {
        _db = db;
        _pricingService = pricingService;
    }

    public async Task<HomeIndexViewModel> GetAsync(CancellationToken ct = default)
    {
        var now = DateTime.Now;

        var products = await _db.TuiXachs
            .AsNoTracking()
            .Include(t => t.ThuongHieu)
            .Include(t => t.LoaiTui)
            .Include(t => t.KhuyenMai)
            .Include(t => t.HinhAnhTuis)
            .Where(t => t.badge != null && (t.badge.Contains("Best Seller") || t.badge.Contains("Bán chạy")))
            .OrderByDescending(t => t.soLuong)
            .Take(4)
            .ToListAsync(ct);

        if (!products.Any())
        {
            products = await _db.TuiXachs
                .AsNoTracking()
                .Include(t => t.ThuongHieu)
                .Include(t => t.LoaiTui)
                .Include(t => t.KhuyenMai)
                .Include(t => t.HinhAnhTuis)
                .OrderByDescending(t => t.soLuong)
                .Take(4)
                .ToListAsync(ct);
        }

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

        return new HomeIndexViewModel
        {
            HotProducts = productCards,
            Brands = brands
        };
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
