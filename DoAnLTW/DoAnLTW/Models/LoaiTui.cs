using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class LoaiTui
    {
        [Key]
        public int maLoai { get; set; }

        [NotMapped]
        public string MaHienThi => $"LT{maLoai:D2}";

        [Required]
        [StringLength(100)]
        public string tenLoai { get; set; } = string.Empty;

        public ICollection<TuiXach> TuiXachs { get; set; } = new List<TuiXach>();
    }
}
