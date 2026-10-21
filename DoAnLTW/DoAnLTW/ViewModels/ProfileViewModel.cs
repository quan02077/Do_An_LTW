using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.ViewModels;

public class ProfileViewModel
{
    public int MaKH { get; set; }

    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Display(Name = "Vai trò tài khoản")]
    public string VaiTro { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng (VD: example@gmail.com).")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
    [Display(Name = "Địa chỉ Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Địa chỉ không vượt quá 255 ký tự.")]
    [Display(Name = "Địa chỉ giao hàng")]
    public string? DiaChi { get; set; }

    public int SoDonHang { get; set; }
    public decimal TongChiTieu { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string MatKhauHienTai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận lại mật khẩu mới.")]
    [DataType(DataType.Password)]
    [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp với mật khẩu mới.")]
    [Display(Name = "Xác nhận mật khẩu mới")]
    public string XacNhanMatKhauMoi { get; set; } = string.Empty;
}
