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

        [StringLength(8)]
        [Required]
        public string matKhau { get; set; } = string.Empty;

        [Required]
        public string vaiTro { get; set; } = string.Empty;

        // Quan hệ 1 - 1 với Khách hàng
        public KhachHang? KhachHang { get; set; }
    }
}
