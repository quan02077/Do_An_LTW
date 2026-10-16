using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("donhang")]
    public class DonHang
    {
        [Key]
        [Column("madh")]
        public int maDH { get; set; }

        [Column("makh")]
        public int maKH { get; set; }
        public KhachHang? KhachHang { get; set; }

        [Column("ngaydat")]
        [Required]
        public DateTime ngayDat { get; set; } = DateTime.Now;

        [Column("tongtien", TypeName = "decimal(15,2)")]
        [Required]
        public decimal tongTien { get; set; }

        [Column("trangthai")]
        [Required]
        [StringLength(50)]
        public string trangThai { get; set; } = string.Empty;

        [Column("diachinhan")]
        [Required]
        [StringLength(255)]
        public string diaChiNhan { get; set; } = string.Empty;

        // Quan hệ 1 - N với Chi tiết đơn hàng (bảng trung gian N-N giữa DonHang và TuiXach)
        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}

