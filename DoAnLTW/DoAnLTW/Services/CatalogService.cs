using DoAnLTW.Data;
using DoAnLTW.Models;
using DoAnLTW.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Services;

public interface ICatalogService
{
    Task<CatalogIndexViewModel> GetCatalogAsync(
        string? category,
        string? brand,
        string? q,
        string? priceRange,
        string? sort,
        CancellationToken ct = default);

    Task<ProductDetailViewModel?> GetDetailAsync(int id, CancellationToken ct = default);
}

public interface IFlashSaleService
{
    Task<FlashSaleIndexViewModel> GetFlashSaleAsync(int? kmId, CancellationToken ct = default);
}

public class CatalogService(DbContext_TuiXach db, IProductPricingService pricingService) : ICatalogService, IFlashSaleService
{
    private static string ResolveImageUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "/HinhAnh/tui1_1.png";
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return path;
        if (path.StartsWith("/")) return path;
        if (path.StartsWith("~")) return path[1..];
        return "/HinhAnh/" + path;
    }

    // 1. Lay danh sach san pham & bo loc
    public async Task<CatalogIndexViewModel> GetCatalogAsync(
        string? category,
        string? brand,
        string? q,
        string? priceRange,
        string? sort,
        CancellationToken ct = default)
    {
        var now = DateTime.Now;

        var brands = await db.ThuongHieus
            .AsNoTracking()
            .Select(b => b.tenThuongHieu)
            .Distinct()
            .ToListAsync(ct);

        var categories = await db.LoaiTuis
            .AsNoTracking()
            .Select(c => c.tenLoai)
            .Distinct()
            .ToListAsync(ct);

        var query = db.TuiXachs
            .AsNoTracking()
            .Include(t => t.ThuongHieu)
            .Include(t => t.LoaiTui)
            .Include(t => t.KhuyenMai)
            .Include(t => t.HinhAnhTuis)
            .AsQueryable();

        // loc theo thuong hieu
        if (!string.IsNullOrWhiteSpace(brand) && brand != "all")
        {
            query = query.Where(t => t.ThuongHieu != null && t.ThuongHieu.tenThuongHieu == brand);
        }

        // loc theo loai tui
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(t => t.LoaiTui != null && t.LoaiTui.tenLoai == category);
        }

        // tim kiem
        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(t => t.tenTui.ToLower().Contains(search) ||
                                    (t.ThuongHieu != null && t.ThuongHieu.tenThuongHieu.ToLower().Contains(search)));
        }

        // loc theo gia
        if (priceRange == "under700k")
        {
            query = query.Where(t => t.donGia < 700000);
        }
        else if (priceRange == "700k-1.2m")
        {
            query = query.Where(t => t.donGia >= 700000 && t.donGia <= 1200000);
        }
        else if (priceRange == "over1.2m")
        {
            query = query.Where(t => t.donGia > 1200000);
        }

        // sap xep
        query = sort switch
        {
            "price-asc" => query.OrderBy(t => t.donGia),
            "price-desc" => query.OrderByDescending(t => t.donGia),
            "stock-desc" => query.OrderByDescending(t => t.soLuong),
            _ => query.OrderByDescending(t => t.maTui)
        };

        var list = await query.ToListAsync(ct);

        var productCards = list.Select(t =>
        {
            var price = pricingService.GetPrice(t, now);
            double phanTram = 0;
            if (price.DonGiaGoc > 0 && price.SoTienGiam > 0)
            {
                phanTram = (double)Math.Round((price.SoTienGiam / price.DonGiaGoc) * 100);
            }

            var mainImg = ResolveImageUrl(t.HinhAnhTuis
                .OrderByDescending(img => img.laAnhChinh)
                .ThenBy(img => img.thuTu)
                .FirstOrDefault()?.tenHinhAnh);

            return new ProductCardItemViewModel(
                t.maTui,
                t.tenTui,
                t.ThuongHieu?.tenThuongHieu ?? "",
                t.LoaiTui?.tenLoai ?? "",
                price.DonGiaGoc,
                price.DonGiaApDung,
                phanTram,
                mainImg,
                t.soLuong,
                t.badge
            );
        }).ToList();

        return new CatalogIndexViewModel
        {
            Products = productCards,
            Brands = brands,
            Categories = categories,
            SelectedBrand = brand,
            SelectedCategory = category,
            SearchQuery = q,
            SelectedPriceRange = priceRange,
            SelectedSort = sort
        };
    }

    // 2. Lay chi tiet 1 san pham kem san pham lien quan
    public async Task<ProductDetailViewModel?> GetDetailAsync(int id, CancellationToken ct = default)
    {
        var now = DateTime.Now;

        var product = await db.TuiXachs
            .AsNoTracking()
            .Include(t => t.ThuongHieu)
            .Include(t => t.LoaiTui)
            .Include(t => t.KhuyenMai)
            .Include(t => t.HinhAnhTuis)
            .FirstOrDefaultAsync(t => t.maTui == id, ct);

        if (product == null) return null;

        var price = pricingService.GetPrice(product, now);
        double phanTram = 0;
        if (price.DonGiaGoc > 0 && price.SoTienGiam > 0)
        {
            phanTram = (double)Math.Round((price.SoTienGiam / price.DonGiaGoc) * 100);
        }

        var images = product.HinhAnhTuis
            .OrderByDescending(img => img.laAnhChinh)
            .ThenBy(img => img.thuTu)
            .Select(img => ResolveImageUrl(img.tenHinhAnh))
            .ToList();

        if (images.Count == 0)
        {
            images.Add("/HinhAnh/tui1_1.png");
        }

        var relatedList = await db.TuiXachs
            .AsNoTracking()
            .Include(t => t.ThuongHieu)
            .Include(t => t.LoaiTui)
            .Include(t => t.KhuyenMai)
            .Include(t => t.HinhAnhTuis)
            .Where(t => t.maLoai == product.maLoai && t.maTui != product.maTui)
            .Take(4)
            .ToListAsync(ct);

        var relatedCards = relatedList.Select(t =>
        {
            var rPrice = pricingService.GetPrice(t, now);
            double rPhanTram = 0;
            if (rPrice.DonGiaGoc > 0 && rPrice.SoTienGiam > 0)
            {
                rPhanTram = (double)Math.Round((rPrice.SoTienGiam / rPrice.DonGiaGoc) * 100);
            }

            var rImg = ResolveImageUrl(t.HinhAnhTuis
                .OrderByDescending(img => img.laAnhChinh)
                .ThenBy(img => img.thuTu)
                .FirstOrDefault()?.tenHinhAnh);

            return new ProductCardItemViewModel(
                t.maTui,
                t.tenTui,
                t.ThuongHieu?.tenThuongHieu ?? "",
                t.LoaiTui?.tenLoai ?? "",
                rPrice.DonGiaGoc,
                rPrice.DonGiaApDung,
                rPhanTram,
                rImg,
                t.soLuong,
                t.badge
            );
        }).ToList();

        return new ProductDetailViewModel
        {
            Product = product,
            DonGiaGoc = price.DonGiaGoc,
            DonGiaHienTai = price.DonGiaApDung,
            PhanTramGiam = phanTram,
            TenKhuyenMai = product.KhuyenMai?.tenKM,
            HinhAnhs = images,
            RelatedProducts = relatedCards
        };
    }

    // 3. Lay danh sach flash sale
    public async Task<FlashSaleIndexViewModel> GetFlashSaleAsync(int? kmId, CancellationToken ct = default)
    {
        var now = DateTime.Now;

        var campaigns = await db.KhuyenMais
            .AsNoTracking()
            .Where(k => k.trangThai)
            .OrderByDescending(k => k.phanTramGiam)
            .ToListAsync(ct);

        var query = db.TuiXachs
            .AsNoTracking()
            .Include(t => t.ThuongHieu)
            .Include(t => t.LoaiTui)
            .Include(t => t.KhuyenMai)
            .Include(t => t.HinhAnhTuis)
            .Where(t => t.maKM != null);

        if (kmId.HasValue && kmId.Value > 0)
        {
            query = query.Where(t => t.maKM == kmId.Value);
        }

        var products = await query.ToListAsync(ct);

        var productCards = products.Select(t =>
        {
            var price = pricingService.GetPrice(t, now);
            double phanTram = 0;
            if (price.DonGiaGoc > 0 && price.SoTienGiam > 0)
            {
                phanTram = (double)Math.Round((price.SoTienGiam / price.DonGiaGoc) * 100);
            }

            var mainImg = ResolveImageUrl(t.HinhAnhTuis
                .OrderByDescending(img => img.laAnhChinh)
                .ThenBy(img => img.thuTu)
                .FirstOrDefault()?.tenHinhAnh);

            return new ProductCardItemViewModel(
                t.maTui,
                t.tenTui,
                t.ThuongHieu?.tenThuongHieu ?? "",
                t.LoaiTui?.tenLoai ?? "",
                price.DonGiaGoc,
                price.DonGiaApDung,
                phanTram,
                mainImg,
                t.soLuong,
                t.badge
            );
        }).ToList();

        return new FlashSaleIndexViewModel
        {
            Campaigns = campaigns,
            Products = productCards,
            SelectedKmId = kmId
        };
    }
}
