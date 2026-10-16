using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    [Table("chitietdonhang")]
    public class ChiTietDonHang
    {
        [Column("madh")]
        public int maDH { get; set; }
        public DonHang? DonHang { get; set; }

        [Column("matui")]
        public int maTui { get; set; }
        public TuiXach? TuiXach { get; set; }

        [Column("soluong")]
        [Required]
        public int soLuong { get; set; }

        [Column("dongia", TypeName = "decimal(15,2)")]
        [Required]
        public decimal donGia { get; set; }

        [Column("thanhtien", TypeName = "decimal(15,2)")]
        [Required]
        public decimal thanhTien { get; set; }
    }
}

