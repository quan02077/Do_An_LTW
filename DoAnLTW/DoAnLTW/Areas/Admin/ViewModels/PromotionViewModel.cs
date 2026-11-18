using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Areas.Admin.ViewModels
{
    public class PromotionIndexViewModel
    {
        public List<PromotionViewModel> KhuyenMais { get; set; } = new();
        public string? Keyword { get; set; }
    }
    public class PromotionViewModel
    {
        public int MaKM { get; set; }
        [Display(Name = "Mã khuyến mãi")]
        public string MaHienThi => $"KM{MaKM:D2}";

        [Display(Name = "Tên khuyến mãi")]
        public string TenKM { get; set; } = string.Empty;

        [Display(Name = "Phần trăm giảm")]
        public int PhanTramGiam { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        public DateTime NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        public DateTime NgayKetThuc { get; set; }

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = string.Empty;
    }

    public class PromotionFormViewModel
    {
        public int MaKM { get; set; }
        [Display(Name = "Mã khuyến mãi")]
        public string MaHienThi => $"KM{MaKM:D2}";
        [Required(ErrorMessage = "Vui lòng nhập tên khuyến mãi.")]
        [Display(Name = "Tên khuyến mãi")]
        public string TenKM { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập phần trăm giảm.")]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100.")]
        [Display(Name = "Phần trăm giảm")]
        public int PhanTramGiam { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime NgayBatDau { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
        [Display(Name = "Ngày kết thúc")]
        public DateTime NgayKetThuc { get; set; }
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;
    }
}
