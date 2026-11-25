using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Models
{
    /// <summary>một tin nhắn trong cuộc trò chuyện khách hàng - nhân viên.</summary>
    public class TinNhan
    {
        public const int MaxNoiDung = 1000;

        [Key]
        public long maTN { get; set; }

        public int maCTC { get; set; }

        public CuocTroChuyen? CuocTroChuyen { get; set; }

        // true: khách gửi; false: nhân viên gửi
        public bool tuKhach { get; set; }

        // tài khoản nhân viên đã trả lời (null khi khách gửi)
        public int? maTKNhanVien { get; set; }

        [Required]
        [StringLength(MaxNoiDung)]
        public string noiDung { get; set; } = string.Empty;

        public DateTime guiLuc { get; set; } = DateTime.Now;

        // bên nhận đã mở xem chưa: dùng để đếm tin chưa đọc (khách gửi -> nhân viên đọc, nhân viên gửi -> khách đọc)
        public bool daDoc { get; set; }
    }
}
