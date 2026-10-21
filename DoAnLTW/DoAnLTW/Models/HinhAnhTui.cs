using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class HinhAnhTui
    {
        [Key]
        public int maHinhAnh { get; set; }

        [NotMapped]
        public string MaHienThi => $"HA{maHinhAnh:D2}";

        [Required]
        public int maTui { get; set; }
        public TuiXach? TuiXach { get; set; }

        [Required]
        [StringLength(255)]
        public string tenHinhAnh { get; set; } = string.Empty;

        [Required]
        public bool laAnhChinh { get; set; } = false;

        [Required]
        public int thuTu { get; set; } = 0;
    }
}
