using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class DonHang
    {
        [Key]
        public int maDH { get; set; }

        [NotMapped]
        public string MaHienThi => $"DH{maDH:D2}";

        [Required]
        public int maKH { get; set; }
        public KhachHang? KhachHang { get; set; }

        [Required]
        public DateTime ngayDat { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal tongTien { get; set; }

        [Required]
        [StringLength(50)]
        public string trangThai { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string diaChiNhan { get; set; } = string.Empty;

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
