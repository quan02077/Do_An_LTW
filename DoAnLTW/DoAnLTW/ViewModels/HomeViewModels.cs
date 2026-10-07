namespace DoAnLTW.ViewModels;

public record ProductCardItemViewModel(
    Guid MaTui,
    string TenTui,
    string TenThuongHieu,
    string TenLoai,
    decimal DonGiaGoc,
    decimal DonGiaHienTai,
    double PhanTramGiam,
    string? HinhAnh,
    int SoLuongTonKho
);

public record BrandItemViewModel(
    Guid MaThuongHieu,
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
