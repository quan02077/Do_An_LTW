using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class DonHang
    {
        [Key]
        public Guid maDH { get; set; }

        public Guid maKH { get; set; }
        public KhachHang? KhachHang { get; set; }

        [Required]
        public DateTime ngayDat { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal tongTien { get; set; }

        [Required]
        [StringLength(50)]
        public string trangThai { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string diaChiNhan { get; set; } = string.Empty;

        // Quan hệ 1 - N với Chi tiết đơn hàng (bảng trung gian N-N giữa DonHang và TuiXach)
        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
