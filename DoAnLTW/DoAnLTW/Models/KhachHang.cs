using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class KhachHang
    {
        [Key]
        public int maKH { get; set; }

        [NotMapped]
        public string MaHienThi => $"KH{maKH:D2}";

        [Required]
        public int maTK { get; set; }

        [NotMapped]
        public int matk
        {
            get => maTK;
            set => maTK = value;
        }

        public TaiKhoan? TaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        public string hoTen { get; set; } = string.Empty;

        [StringLength(100)]
        public string? email { get; set; }

        [StringLength(15)]
        public string? sdt { get; set; }

        [StringLength(255)]
        public string? diaChi { get; set; }

        public GioHang? GioHang { get; set; }

        public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    }
}
