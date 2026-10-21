using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class TuiXach
    {
        [Key]
        public int maTui { get; set; }

        [NotMapped]
        public string MaHienThi => $"TX{maTui:D2}";

        [Required]
        [StringLength(150)]
        public string tenTui { get; set; } = string.Empty;

        [Required]
        public int maThuongHieu { get; set; }
        public ThuongHieu? ThuongHieu { get; set; }

        [Required]
        public int maLoai { get; set; }
        public LoaiTui? LoaiTui { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal donGia { get; set; }

        [Required]
        public int soLuong { get; set; }

        public string? moTa { get; set; }

        [StringLength(50)]
        public string? badge { get; set; }

        public int? maKM { get; set; }
        public KhuyenMai? KhuyenMai { get; set; }

        public ICollection<HinhAnhTui> HinhAnhTuis { get; set; } = new List<HinhAnhTui>();
        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
