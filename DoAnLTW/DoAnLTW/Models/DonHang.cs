using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class DonHang
    {
        [Key]
        public int maDH { get; set; }

        [NotMapped]
        public string MaHienThi => $"DH{maDH:D2}";

        [Required]
        public int maKH { get; set; }
        public KhachHang? KhachHang { get; set; }

        [Required]
        public DateTime ngayDat { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal tongTien { get; set; }

        [Required]
        [StringLength(50)]
        public string trangThai { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string diaChiNhan { get; set; } = string.Empty;

        // thông tin người nhận có thể khác chủ tài khoản (mua tặng, nhờ người nhận hộ)
        [Required]
        [StringLength(100)]
        public string tenNguoiNhan { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string sdtNguoiNhan { get; set; } = string.Empty;

        // mã phương thức thanh toán: xem PaymentMethod trong Services/OrderService.cs
        [Required]
        [StringLength(20)]
        public string phuongThucThanhToan { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ghiChu { get; set; }

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
