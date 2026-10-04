using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Models
{
    public class KhachHang
    {
        [Key]
        public Guid maKH { get; set; }

        [Required]
        [StringLength(50)]
        public string hoTen { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string email { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string sdt { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string diaChi { get; set; } = string.Empty;

        public Guid matk { get; set; }
        
        public TaiKhoan? TaiKhoan { get; set; }

        // Quan hệ 1 - 1 với Giỏ hàng
        public GioHang? GioHang { get; set; }

        // Quan hệ 1 - N với Đơn hàng
        public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    }
}
