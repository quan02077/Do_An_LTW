using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class GioHang
    {
        [Key]
        public int maGioHang { get; set; }

        [NotMapped]
        public string MaHienThi => $"GH{maGioHang:D2}";

        [Required]
        public int maKH { get; set; }
        public KhachHang? KhachHang { get; set; }

        [Required]
        public DateTime ngayTao { get; set; } = DateTime.Now;

        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
    }
}
