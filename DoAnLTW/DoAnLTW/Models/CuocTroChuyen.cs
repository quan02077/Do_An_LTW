using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Models
{
    /// <summary>cuộc trò chuyện giữa một khách hàng và nhân viên cửa hàng (mỗi khách đúng một cuộc, nối dài theo thời gian).</summary>
    public class CuocTroChuyen
    {
        [Key]
        public int maCTC { get; set; }

        public int maKH { get; set; }

        public KhachHang? KhachHang { get; set; }

        // thời điểm của tin nhắn mới nhất, dùng để sắp xếp hộp thư của nhân viên
        public DateTime capNhatLuc { get; set; } = DateTime.Now;

        public ICollection<TinNhan> TinNhans { get; set; } = new List<TinNhan>();
    }
}
