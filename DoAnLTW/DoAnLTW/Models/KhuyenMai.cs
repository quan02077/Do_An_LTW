using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class KhuyenMai
    {
        [Key]
        public int maKM { get; set; }

        [NotMapped]
        public string MaHienThi => $"KM{maKM:D2}";

        [Required]
        [StringLength(100)]
        public string tenKM { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal phanTramGiam { get; set; }

        [Required]
        public DateTime ngayBatDau { get; set; }

        [Required]
        public DateTime ngayKetThuc { get; set; }

        [Required]
        public bool trangThai { get; set; } = true;

        public ICollection<TuiXach> TuiXachs { get; set; } = new List<TuiXach>();
    }
}
