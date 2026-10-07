using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class TuiXach
    {
        [Key]
        public Guid maTui { get; set; }

        [Required]
        [StringLength(200)]
        public string tenTui { get; set; } = string.Empty;

        public Guid maThuongHieu { get; set; }
        public ThuongHieu? ThuongHieu { get; set; }

        public Guid maLoai { get; set; }
        public LoaiTui? LoaiTui { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal donGia { get; set; }

        [Required]
        public int soLuong { get; set; }

        [StringLength(1000)]
        public string? moTa { get; set; }

        [StringLength(50)]
        public string? badge { get; set; }

        public Guid? maKM { get; set; }
        public KhuyenMai? KhuyenMai { get; set; }

        // Quan hệ 1 - N với Hình ảnh túi
        public ICollection<HinhAnhTui> HinhAnhTuis { get; set; } = new List<HinhAnhTui>();

        // Quan hệ 1 - N với Chi tiết giỏ hàng (bảng trung gian N-N giữa TuiXach và GioHang)
        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

        // Quan hệ 1 - N với Chi tiết đơn hàng (bảng trung gian N-N giữa TuiXach và DonHang)
        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
