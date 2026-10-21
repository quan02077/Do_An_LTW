using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class TaiKhoan
    {
        [Key]
        public int maTK { get; set; }

        [NotMapped]
        public string MaHienThi => $"TK{maTK:D2}";

        [Required]
        [StringLength(50)]
        public string tenDangNhap { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string matKhau { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string vaiTro { get; set; } = string.Empty;

        public KhachHang? KhachHang { get; set; }
    }
}
