using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class ChiTietDonHang
    {
        public int maDH { get; set; }
        public DonHang? DonHang { get; set; }

        public int maTui { get; set; }
        public TuiXach? TuiXach { get; set; }

        public int soLuong { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal donGia { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal thanhTien { get; set; }
    }
}
