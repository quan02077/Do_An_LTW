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

        [Display(Name = "Số sản phẩm áp dụng")]
        public int SoSanPham { get; set; }
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

        // mã các túi xách được tick checkbox để áp dụng khuyến mãi này
        [Display(Name = "Sản phẩm áp dụng")]
        public List<int> SelectedMaTui { get; set; } = new();

        // danh sách hiển thị checkbox (không gửi từ form, service nạp lại mỗi lần hiển thị)
        public List<PromotionProductOption> SanPhamOptions { get; set; } = new();
    }

    // một dòng trong danh sách checkbox chọn sản phẩm
    public class PromotionProductOption
    {
        public int MaTui { get; set; }
        public string MaHienThi => $"TX{MaTui:D2}";
        public string TenTui { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public string? HinhAnh { get; set; }

        // khuyến mãi mà túi đang thuộc (null = chưa có khuyến mãi)
        public int? MaKMHienTai { get; set; }
        public string? TenKMHienTai { get; set; }
    }
}
