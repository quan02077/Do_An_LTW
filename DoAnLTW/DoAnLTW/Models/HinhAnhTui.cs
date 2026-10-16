using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("hinhanhtui")]
    public class HinhAnhTui
    {
        [Key]
        [Column("mahinhanh")]
        public int maHinhAnh { get; set; }

        [Column("matui")]
        public int maTui { get; set; }
        public TuiXach? TuiXach { get; set; }

        [Column("tenhinhanh")]
        [Required]
        [StringLength(255)]
        public string tenHinhAnh { get; set; } = string.Empty;

        [Column("laanhchinh")]
        [Required]
        public bool laAnhChinh { get; set; } = false;

        [Column("thutu")]
        [Required]
        public int thuTu { get; set; } = 0;
    }
}

