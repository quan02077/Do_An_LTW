using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("thuonghieu")]
    public class ThuongHieu
    {
        [Key]
        [Column("mathuonghieu")]
        public int maThuongHieu { get; set; }

        [Column("tenthuonghieu")]
        [Required]
        [StringLength(100)]
        public string tenThuongHieu { get; set; } = string.Empty;

        // Quan hệ 1 - N với Túi xách
        public ICollection<TuiXach> TuiXachs { get; set; } = new List<TuiXach>();
    }
}

