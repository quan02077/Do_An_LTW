using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.ViewModels;

public class ProfileViewModel
{
    public int MaKH { get; set; }

    [Display(Name = "tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Display(Name = "vai trò tài khoản")]
    public string VaiTro { get; set; } = string.Empty;

    [Required(ErrorMessage = "vui lòng nhập họ và tên.")]
    [StringLength(100, ErrorMessage = "họ và tên không được vượt quá 100 ký tự.")]
    [Display(Name = "họ và tên")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "vui lòng nhập địa chỉ email.")]
    [EmailAddress(ErrorMessage = "địa chỉ email không đúng định dạng (vd: example@gmail.com).")]
    [StringLength(100, ErrorMessage = "email không được vượt quá 100 ký tự.")]
    [Display(Name = "địa chỉ email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "vui lòng nhập số điện thoại.")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0.")]
    [Display(Name = "số điện thoại")]
    public string SoDienThoai { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "địa chỉ không vượt quá 255 ký tự.")]
    [Display(Name = "địa chỉ giao hàng")]
    public string? DiaChi { get; set; }

    public int SoDonHang { get; set; }
    public decimal TongChiTieu { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "vui lòng nhập mật khẩu hiện tại.")]
    [DataType(DataType.Password)]
    [Display(Name = "mật khẩu hiện tại")]
    public string MatKhauHienTai { get; set; } = string.Empty;

    [Required(ErrorMessage = "vui lòng nhập mật khẩu mới.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "mật khẩu mới phải có ít nhất 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "mật khẩu mới")]
    public string MatKhauMoi { get; set; } = string.Empty;

    [Required(ErrorMessage = "vui lòng xác nhận lại mật khẩu mới.")]
    [DataType(DataType.Password)]
    [Compare("MatKhauMoi", ErrorMessage = "mật khẩu xác nhận không khớp với mật khẩu mới.")]
    [Display(Name = "xác nhận mật khẩu mới")]
    public string XacNhanMatKhauMoi { get; set; } = string.Empty;
}
