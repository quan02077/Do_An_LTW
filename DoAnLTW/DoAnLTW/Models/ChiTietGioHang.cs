using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class ChiTietGioHang
    {
        public Guid maGioHang { get; set; }
        public GioHang? GioHang { get; set; }

        public Guid maTui { get; set; }
        public TuiXach? TuiXach { get; set; }

        [Required]
        public int soLuong { get; set; }
    }
}
