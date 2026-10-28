using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.ViewModels;
public class CategoryIndexViewModel
{
    public List<TypeViewModel> LoaiTuis { get; set; } = new();
    public List<BrandViewModel> ThuongHieus { get; set; } = new();
}

// hien thi loai tui
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

// them moi loai tui
public class TypeCreateViewModel
{
    [Required(ErrorMessage = "Tên loại túi không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên loại túi không được vượt quá 100 ký tự.")]
    [Display(Name = "Tên loại túi")]
    public string TenLoai { get; set; } = string.Empty;
}

// chinh sua loai tui
public class TypeEditViewModel
{
    public int MaLoai { get; set; }

    [Display(Name = "Mã hiển thị")]
    public string MaHienThi => $"LT{MaLoai:D2}";

    [Required(ErrorMessage = "Tên loại túi không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên loại túi không được vượt quá 100 ký tự.")]
    [Display(Name = "Tên loại túi")]
    public string TenLoai { get; set; } = string.Empty;

    public int SoLuongSanPham { get; set; }
}

// hien thi thuong hieu
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

// them moi thuong hieu
public class BrandCreateViewModel
{
    [Required(ErrorMessage = "Tên thương hiệu không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên thương hiệu không được vượt quá 100 ký tự.")]
    [Display(Name = "Tên thương hiệu")]
    public string TenThuongHieu { get; set; } = string.Empty;
}

// chinh sua thuong hieu
public class BrandEditViewModel
{
    public int MaThuongHieu { get; set; }

    [Display(Name = "Mã hiển thị")]
    public string MaHienThi => $"TH{MaThuongHieu:D2}";

    [Required(ErrorMessage = "Tên thương hiệu không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên thương hiệu không được vượt quá 100 ký tự.")]
    [Display(Name = "Tên thương hiệu")]
    public string TenThuongHieu { get; set; } = string.Empty;

    public int SoLuongSanPham { get; set; }
}
