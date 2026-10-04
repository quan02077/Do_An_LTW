using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Models
{
    public class HinhAnhTui
    {
        [Key]
        public Guid maHinhAnh { get; set; }

        public Guid maTui { get; set; }
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
