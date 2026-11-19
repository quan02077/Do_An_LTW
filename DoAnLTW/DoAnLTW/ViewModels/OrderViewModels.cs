using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.ViewModels;

public class CheckoutViewModel
{
    // tùy chọn khi chọn mua: chỉ thanh toán những sản phẩm được tick trong giỏ
    [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.")]
    public List<int> SelectedMaTui { get; set; } = new();

    [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
    [StringLength(100, ErrorMessage = "Tên người nhận không quá 100 ký tự.")]
    [Display(Name = "Tên người nhận")]
    public string TenNguoiNhan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại người nhận.")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0.")]
    [Display(Name = "Số điện thoại")]
    public string SdtNguoiNhan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
    [StringLength(255, ErrorMessage = "Địa chỉ không quá 255 ký tự.")]
    [Display(Name = "Địa chỉ nhận hàng")]
    public string DiaChiNhan { get; set; } = string.Empty;

    // tùy chọn khi đặt hàng: giá trị hợp lệ được service kiểm tra lại bằng PaymentMethod.IsValid
    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
    [Display(Name = "Phương thức thanh toán")]
    public string PhuongThucThanhToan { get; set; } = "COD";

    [StringLength(500, ErrorMessage = "Ghi chú không quá 500 ký tự.")]
    [Display(Name = "Ghi chú cho người bán")]
    public string? GhiChu { get; set; }
}

public record OrderItemViewModel(
    int MaTui,
    string TenTui,
    string? Anh,
    int SoLuong,
    decimal DonGia,
    decimal ThanhTien)
{
    public string MaHienThi => $"TX{MaTui:D2}";
}

public record OrderViewModel(
    int MaDonHang,
    DateTime NgayDat,
    string TrangThai,
    string DiaChiNhan,
    decimal TongTien,
    IReadOnlyCollection<OrderItemViewModel> Items,
    string TenNguoiNhan = "",
    string SdtNguoiNhan = "",
    string PhuongThucThanhToan = "",
    string? GhiChu = null)
{
    public string MaHienThi => $"DH{MaDonHang:D2}";
}

public record CheckoutPageViewModel(CartViewModel Cart, CheckoutViewModel Checkout);
