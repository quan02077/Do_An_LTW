using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("taikhoan")]
    public class TaiKhoan
    {
        [Key]
        [Column("matk")]
        public int maTK { get; set; }

        [Column("tendangnhap")]
        [StringLength(50)]
        [Required]
        public string tenDangNhap { get; set; } = string.Empty;

        // PasswordHasher của ASP.NET Core tạo chuỗi hash dài hơn 8 ký tự.
        [Column("matkhau")]
        [StringLength(255)]
        [Required]
        public string matKhau { get; set; } = string.Empty;

        [Column("vaitro")]
        [Required]
        public string vaiTro { get; set; } = string.Empty;

        // Quan hệ 1 - 1 với Khách hàng
        public KhachHang? KhachHang { get; set; }
    }
}

