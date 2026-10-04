using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Models
{
    public class GioHang
    {
        [Key]
        public Guid maGioHang { get; set; }

        public Guid maKH { get; set; }
        public KhachHang? KhachHang { get; set; }

        [Required]
        public DateTime ngayTao { get; set; } = DateTime.Now;

        // Quan hệ 1 - N với Chi tiết giỏ hàng (bảng trung gian N-N giữa GioHang và TuiXach)
        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
    }
}
