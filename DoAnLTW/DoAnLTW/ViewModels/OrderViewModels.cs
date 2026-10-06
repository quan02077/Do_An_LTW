using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.ViewModels;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
    [StringLength(255)]
    [Display(Name = "Địa chỉ nhận hàng")]
    public string DiaChiNhan { get; set; } = string.Empty;
}

public record OrderItemViewModel(
    Guid MaTui,
    string TenTui,
    string? Anh,
    int SoLuong,
    decimal DonGia,
    decimal ThanhTien);

public record OrderViewModel(
    Guid MaDonHang,
    DateTime NgayDat,
    string TrangThai,
    string DiaChiNhan,
    decimal TongTien,
    IReadOnlyCollection<OrderItemViewModel> Items);

public record CheckoutPageViewModel(CartViewModel Cart, CheckoutViewModel Checkout);
