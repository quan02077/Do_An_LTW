using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("giohang")]
    public class GioHang
    {
        [Key]
        [Column("magiohang")]
        public int maGioHang { get; set; }

        [Column("makh")]
        public int maKH { get; set; }
        public KhachHang? KhachHang { get; set; }

        [Column("ngaytao")]
        [Required]
        public DateTime ngayTao { get; set; } = DateTime.Now;

        // Quan hệ 1 - N với Chi tiết giỏ hàng (bảng trung gian N-N giữa GioHang và TuiXach)
        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
    }
}

