using System.ComponentModel.DataAnnotations;
using DoAnLTW.Models;

namespace DoAnLTW.Areas.Admin.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public decimal PendingAmount { get; set; }
    public int DeliveredOrders { get; set; }
    public decimal DeliveredAmount { get; set; }
    public int CanceledOrders { get; set; }
    public decimal CanceledAmount { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalBrands { get; set; }
    public List<DonHang> RecentOrders { get; set; } = new();
}

public class ProductCreateViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên túi xách")]
    [StringLength(150)]
    public string TenTui { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại túi")]
    public int MaLoai { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thương hiệu")]
    public int MaThuongHieu { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập đơn giá")]
    [Range(1000, 100000000, ErrorMessage = "Giá bán phải từ 1.000đ trở lên")]
    public decimal DonGia { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số lượng")]
    [Range(0, 100000, ErrorMessage = "Số lượng không hợp lệ")]
    public int SoLuong { get; set; } = 10;

    public string? MoTa { get; set; }
    public string? Badge { get; set; }
}

public class ProductEditViewModel
{
    public int MaTui { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên túi xách")]
    [StringLength(150)]
    public string TenTui { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại túi")]
    public int MaLoai { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thương hiệu")]
    public int MaThuongHieu { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập đơn giá")]
    [Range(1000, 100000000, ErrorMessage = "Giá bán phải từ 1.000đ trở lên")]
    public decimal DonGia { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số lượng")]
    [Range(0, 100000, ErrorMessage = "Số lượng không hợp lệ")]
    public int SoLuong { get; set; }

    public string? MoTa { get; set; }
    public string? Badge { get; set; }
    public string? AnhHienTai { get; set; }
}
