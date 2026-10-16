using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("khuyenmai")]
    public class KhuyenMai
    {
        [Key]
        [Column("makm")]
        public int maKM { get; set; }

        [Column("tenkm")]
        [Required]
        [StringLength(100)]
        public string tenKM { get; set; } = string.Empty;

        [Column("phantramgiam", TypeName = "decimal(5,2)")]
        [Required]
        public decimal phanTramGiam { get; set; }

        [Column("ngaybatdau")]
        [Required]
        public DateTime ngayBatDau { get; set; }

        [Column("ngayketthuc")]
        [Required]
        public DateTime ngayKetThuc { get; set; }

        [Column("trangthai")]
        [Required]
        public bool trangThai { get; set; } = true;

        // Quan hệ 1 - N với Túi xách
        public ICollection<TuiXach> TuiXachs { get; set; } = new List<TuiXach>();
    }
}

