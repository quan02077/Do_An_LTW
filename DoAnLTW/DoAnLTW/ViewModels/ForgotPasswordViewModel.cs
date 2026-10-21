using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email đã đăng ký.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng (VD: example@gmail.com).")]
    [Display(Name = "Địa chỉ Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận lại mật khẩu mới.")]
    [DataType(DataType.Password)]
    [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp với mật khẩu mới.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string XacNhanMatKhau { get; set; } = string.Empty;
}
