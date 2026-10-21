using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class ThuongHieu
    {
        [Key]
        public int maThuongHieu { get; set; }

        [NotMapped]
        public string MaHienThi => $"TH{maThuongHieu:D2}";

        [Required]
        [StringLength(100)]
        public string tenThuongHieu { get; set; } = string.Empty;

        public ICollection<TuiXach> TuiXachs { get; set; } = new List<TuiXach>();
    }
}
