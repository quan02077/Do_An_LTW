using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoAnLTW.ViewModels;

// 1. Dùng cho trang danh sách sản phẩm (Index / List)
public class ProductIndexViewModel
{
    public List<ProductViewModel> SanPhams { get; set; } = new();

    // bộ lọc tìm kiếm nhanh
    public string? Keyword { get; set; }
    public int? SelectedMaLoai { get; set; }
    public int? SelectedMaThuongHieu { get; set; }
    public List<SelectListItem> LoaiTuiOptions { get; set; } = new();
    public List<SelectListItem> ThuongHieuOptions { get; set; } = new();
}

// 2. Dùng để hiển thị từng dòng trong bảng sản phẩm
public class ProductViewModel
{
    public int MaTui { get; set; }

    [Display(Name = "Mã hiển thị")]
    public string MaHienThi => $"TX{MaTui:D2}";

    [Display(Name = "Tên túi xách")]
    public string TenTui { get; set; } = string.Empty;

    [Display(Name = "Thương hiệu")]
    public string TenThuongHieu { get; set; } = string.Empty;

    [Display(Name = "Loại túi")]
    public string TenLoai { get; set; } = string.Empty;

    [Display(Name = "Đơn giá")]
    public decimal DonGia { get; set; }

    [Display(Name = "Số lượng")]
    public int SoLuong { get; set; }

    [Display(Name = "Hình ảnh")]
    public string? HinhAnhChinh { get; set; }

    [Display(Name = "Nhãn")]
    public string? Badge { get; set; }

    public bool ConHang => SoLuong > 0;
}

// 3. Dùng chung cho Form Thêm mới và Chỉnh sửa sản phẩm
public class ProductFormViewModel
{
    public int MaTui { get; set; }

    [Display(Name = "Mã hiển thị")]
    public string MaHienThi => $"TX{MaTui:D2}";

    [Required(ErrorMessage = "Vui lòng nhập tên túi xách.")]
    [StringLength(150, ErrorMessage = "Tên túi xách không được vượt quá 150 ký tự.")]
    [Display(Name = "Tên túi xách")]
    public string TenTui { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại túi.")]
    [Display(Name = "Loại túi")]
    public int MaLoai { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thương hiệu.")]
    [Display(Name = "Thương hiệu")]
    public int MaThuongHieu { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập đơn giá.")]
    [Range(1000, 1000000000, ErrorMessage = "Đơn giá phải từ 1.000đ đến 1.000.000.000đ.")]
    [Display(Name = "Đơn giá (VNĐ)")]
    public decimal DonGia { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số lượng kho.")]
    [Range(0, 100000, ErrorMessage = "Số lượng kho phải từ 0 đến 100.000.")]
    [Display(Name = "Số lượng trong kho")]
    public int SoLuong { get; set; }

    [Display(Name = "Mô tả sản phẩm")]
    public string? MoTa { get; set; }

    [StringLength(50, ErrorMessage = "Nhãn không được vượt quá 50 ký tự.")]
    [Display(Name = "Nhãn (Hot, New, Sale...)")]
    public string? Badge { get; set; }

    // hình ảnh
    [Display(Name = "Ảnh đại diện hiện tại")]
    public string? HinhAnhHienTai { get; set; }

    [Display(Name = "Tải lên ảnh mới")]
    public IFormFile? FileHinhAnh { get; set; }

    // danh sách chọn cho dropdown (select box)
    public List<SelectListItem> LoaiTuiOptions { get; set; } = new();
    public List<SelectListItem> ThuongHieuOptions { get; set; } = new();
}
