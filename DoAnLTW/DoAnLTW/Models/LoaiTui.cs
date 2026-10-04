using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Models
{
    public class LoaiTui
    {
        [Key]
        public Guid maLoai { get; set; }

        [Required]
        [StringLength(100)]
        public string tenLoai { get; set; } = string.Empty;

        // Quan hệ 1 - N với Túi xách
        public ICollection<TuiXach> TuiXachs { get; set; } = new List<TuiXach>();
    }
}
