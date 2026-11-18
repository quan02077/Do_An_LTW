using DoAnLTW.Models;

namespace DoAnLTW.ViewModels
{
    // viewmodel cho trang danh sach san pham & bo loc
    public class CatalogIndexViewModel
    {
        public List<ProductCardItemViewModel> Products { get; set; } = new();
        public List<string> Brands { get; set; } = new();
        public List<string> Categories { get; set; } = new();

        // cac tieu chi dang loc
        public string? SelectedCategory { get; set; }
        public string? SelectedBrand { get; set; }
        public string? SearchQuery { get; set; }
        public string? SelectedPriceRange { get; set; }
        public string? SelectedSort { get; set; }
    }

    // viewmodel cho trang chi tiet san pham
    public class ProductDetailViewModel
    {
        public TuiXach Product { get; set; } = null!;
        public decimal DonGiaGoc { get; set; }
        public decimal DonGiaHienTai { get; set; }
        public double PhanTramGiam { get; set; }
        public string? TenKhuyenMai { get; set; }
        public List<string> HinhAnhs { get; set; } = new();
        public List<ProductCardItemViewModel> RelatedProducts { get; set; } = new();
    }

    // viewmodel cho trang flash sale / khuyen mai
    public class FlashSaleIndexViewModel
    {
        public List<KhuyenMai> Campaigns { get; set; } = new();
        public List<ProductCardItemViewModel> Products { get; set; } = new();
        public int? SelectedKmId { get; set; }
    }
}
