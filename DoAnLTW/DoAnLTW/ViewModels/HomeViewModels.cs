namespace DoAnLTW.ViewModels;

public record ProductCardItemViewModel(
    int MaTui,
    string TenTui,
    string TenThuongHieu,
    string TenLoai,
    decimal DonGiaGoc,
    decimal DonGiaHienTai,
    double PhanTramGiam,
    string? HinhAnh,
    int SoLuongTonKho,
    string? Badge = null
);

public record BrandItemViewModel(
    int MaThuongHieu,
    string TenThuongHieu,
    int SoLuongSanPham,
    string? MoTa = null,
    string? LogoUrl = null
);

public class HomeIndexViewModel
{
    public List<ProductCardItemViewModel> HotProducts { get; set; } = new();
    public List<BrandItemViewModel> Brands { get; set; } = new();
}
