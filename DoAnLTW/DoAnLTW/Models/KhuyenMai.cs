using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Models
{
    public class KhuyenMai
    {
        [Key]
        public Guid maKM { get; set; }

        [Required]
        [StringLength(100)]
        public string tenKM { get; set; } = string.Empty;

        [Required]
        public double phanTramGiam { get; set; }

        [Required]
        public DateTime ngayBatDau { get; set; }

        [Required]
        public DateTime ngayKetThuc { get; set; }

        [Required]
        public bool trangThai { get; set; } = true;

        // Quan hệ 1 - N với Túi xách
        public ICollection<TuiXach> TuiXachs { get; set; } = new List<TuiXach>();
    }
}
