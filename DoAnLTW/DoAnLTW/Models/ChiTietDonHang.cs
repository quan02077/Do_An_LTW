using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class ChiTietDonHang
    {
        public Guid maDH { get; set; }
        public DonHang? DonHang { get; set; }

        public Guid maTui { get; set; }
        public TuiXach? TuiXach { get; set; }

        [Required]
        public int soLuong { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal donGia { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal thanhTien { get; set; }
    }
}
