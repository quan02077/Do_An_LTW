using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.ViewModels;

public class AddCartItemViewModel
{
    [Required] public Guid MaTui { get; set; }
    [Range(1, 99)] public int SoLuong { get; set; } = 1;
}

public class UpdateCartItemViewModel
{
    [Range(1, 99)] public int SoLuong { get; set; }
}

public record CartItemViewModel(Guid MaTui, string TenTui, int SoLuong, decimal DonGia, decimal ThanhTien, string? Anh);
public record CartViewModel(IReadOnlyCollection<CartItemViewModel> Items, int TongSoLuong, decimal TongTien);
