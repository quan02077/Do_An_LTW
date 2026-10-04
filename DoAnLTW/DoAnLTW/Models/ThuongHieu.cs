using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Models
{
    public class ThuongHieu
    {
        [Key]
        public Guid maThuongHieu { get; set; }

        [Required]
        [StringLength(100)]
        public string tenThuongHieu { get; set; } = string.Empty;

        // Quan hệ 1 - N với Túi xách
        public ICollection<TuiXach> TuiXachs { get; set; } = new List<TuiXach>();
    }
}
