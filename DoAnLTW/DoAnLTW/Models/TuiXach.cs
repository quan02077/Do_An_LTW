using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("tuixach")]
    public class TuiXach
    {
        [Key]
        [Column("matui")]
        public int maTui { get; set; }

        [Column("tentui")]
        [Required]
        [StringLength(150)]
        public string tenTui { get; set; } = string.Empty;

        [Column("mathuonghieu")]
        public int maThuongHieu { get; set; }
        public ThuongHieu? ThuongHieu { get; set; }

        [Column("maloai")]
        public int maLoai { get; set; }
        public LoaiTui? LoaiTui { get; set; }

        [Column("dongia", TypeName = "decimal(15,2)")]
        [Required]
        public decimal donGia { get; set; }

        [Column("soluong")]
        [Required]
        public int soLuong { get; set; }

        [Column("mota")]
        public string? moTa { get; set; }

        [NotMapped]
        public string? badge { get; set; }

        [Column("makm")]
        public int? maKM { get; set; }
        public KhuyenMai? KhuyenMai { get; set; }

        // Quan hệ 1 - N với Hình ảnh túi
        public ICollection<HinhAnhTui> HinhAnhTuis { get; set; } = new List<HinhAnhTui>();

        // Quan hệ 1 - N với Chi tiết giỏ hàng (bảng trung gian N-N giữa TuiXach và GioHang)
        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

        // Quan hệ 1 - N với Chi tiết đơn hàng (bảng trung gian N-N giữa TuiXach và DonHang)
        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}

