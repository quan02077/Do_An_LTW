using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("chitietgiohang")]
    public class ChiTietGioHang
    {
        [Column("magiohang")]
        public int maGioHang { get; set; }
        public GioHang? GioHang { get; set; }

        [Column("matui")]
        public int maTui { get; set; }
        public TuiXach? TuiXach { get; set; }

        [Column("soluong")]
        [Required]
        public int soLuong { get; set; }
    }
}
