using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("loaitui")]
    public class LoaiTui
    {
        [Key]
        [Column("maloai")]
        public int maLoai { get; set; }

        [Column("tenloai")]
        [Required]
        [StringLength(100)]
        public string tenLoai { get; set; } = string.Empty;

        // Quan hệ 1 - N với Túi xách
        public ICollection<TuiXach> TuiXachs { get; set; } = new List<TuiXach>();
    }
}

