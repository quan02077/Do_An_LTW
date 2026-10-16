using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("khachhang")]
    public class KhachHang
    {
        [Key]
        [Column("makh")]
        public int maKH { get; set; }

        [Column("hoten")]
        [Required]
        [StringLength(100)]
        public string hoTen { get; set; } = string.Empty;

        [Column("email")]
        [StringLength(100)]
        public string? email { get; set; }

        [Column("sodienthoai")]
        [StringLength(15)]
        public string? sdt { get; set; }

        [Column("diachi")]
        [StringLength(255)]
        public string? diaChi { get; set; }

        [Column("matk")]
        public int matk { get; set; }
        
        public TaiKhoan? TaiKhoan { get; set; }

        // Quan hệ 1 - 1 với Giỏ hàng
        public GioHang? GioHang { get; set; }

        // Quan hệ 1 - N với Đơn hàng
        public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    }
}

