using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.ViewModels;

// 1. Dùng cho trang Index hiển thị cả 2 danh sách
public class CategoryIndexViewModel
{
    public List<TypeViewModel> LoaiTuis { get; set; } = new();
    public List<BrandViewModel> ThuongHieus { get; set; } = new();
}

// 2. Dùng chung cho Loại túi (Xem, Thêm, Sửa)
public class TypeViewModel
{
    public int MaLoai { get; set; }

    [Display(Name = "Mã hiển thị")]
    public string MaHienThi => $"LT{MaLoai:D2}";

    [Required(ErrorMessage = "Vui lòng nhập tên loại túi.")]
    [StringLength(100, ErrorMessage = "Tên loại túi không được vượt quá 100 ký tự.")]
    [Display(Name = "Tên loại túi")]
    public string TenLoai { get; set; } = string.Empty;

    [Display(Name = "Số sản phẩm")]
    public int SoLuongSanPham { get; set; }
}

// 3. Dùng chung cho Thương hiệu (Xem, Thêm, Sửa)
public class BrandViewModel
{
    public int MaThuongHieu { get; set; }

    [Display(Name = "Mã hiển thị")]
    public string MaHienThi => $"TH{MaThuongHieu:D2}";

    [Required(ErrorMessage = "Vui lòng nhập tên thương hiệu.")]
    [StringLength(100, ErrorMessage = "Tên thương hiệu không được vượt quá 100 ký tự.")]
    [Display(Name = "Tên thương hiệu")]
    public string TenThuongHieu { get; set; } = string.Empty;

    [Display(Name = "Số sản phẩm")]
    public int SoLuongSanPham { get; set; }
}


