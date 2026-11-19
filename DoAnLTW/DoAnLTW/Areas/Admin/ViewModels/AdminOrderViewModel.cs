using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Areas.Admin.ViewModels
{
    // 1. Dung cho trang danh sach don hang
    public class AdminOrderIndexViewModel
    {
        public List<AdminOrderViewModel> DonHangs { get; set; } = new();
        public string? Keyword { get; set; }
        public string? SelectedTrangThai { get; set; }
        public List<SelectListItem> TrangThaiOptions { get; set; } = new();
    }

    // 2. Hien thi tung don hang tren bang danh sach
    public class AdminOrderViewModel
    {
        public int MaDH { get; set; }
        [Display(Name = "Mã đơn hàng")]
        public string MaHienThi => $"DH{MaDH:D2}";

        public int MaKH { get; set; }
        [Display(Name = "Mã khách hàng")]
        public string MaKHDisplay => $"KH{MaKH:D2}";

        [Display(Name = "Tên khách hàng")]
        public string TenKH { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "SĐT")]
        public string SDT { get; set; } = string.Empty;

        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ nhận")]
        public string DiaChiNhan { get; set; } = string.Empty;
    }

    // 3. Dung cho trang xem chi tiet va cap nhat trang thai
    public class AdminOrderFormViewModel
    {
        public int MaDH { get; set; }
        [Display(Name = "Mã đơn hàng")]
        public string MaHienThi => $"DH{MaDH:D2}";

        public int MaKH { get; set; }
        [Display(Name = "Mã khách hàng")]
        public string MaKHDisplay => $"KH{MaKH:D2}";

        [Display(Name = "Tên khách hàng")]
        public string TenKH { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "SĐT")]
        public string SDT { get; set; } = string.Empty;

        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ nhận")]
        public string DiaChiNhan { get; set; } = string.Empty;

        public List<SelectListItem> TrangThaiOptions { get; set; } = new();
        public List<AdminOrderItemViewModel> ChiTietDonHangs { get; set; } = new();
    }

    // 4. Tung dong san pham trong chi tiet don hang
    public class AdminOrderItemViewModel
    {
        public int MaTui { get; set; }
        public string TenTui { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}
