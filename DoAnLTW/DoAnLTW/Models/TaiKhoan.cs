using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Models
{
    public class TaiKhoan
    {
        [Key]
        public Guid maTK { get; set; }

        [StringLength(50)]
        [Required]
        public string tenDangNhap { get; set; } = string.Empty;

        // PasswordHasher của ASP.NET Core tạo chuỗi hash dài hơn 8 ký tự.
        [StringLength(255)]
        [Required]
        public string matKhau { get; set; } = string.Empty;

        [Required]
        public string vaiTro { get; set; } = string.Empty;

        // Quan hệ 1 - 1 với Khách hàng
        public KhachHang? KhachHang { get; set; }
    }
}
