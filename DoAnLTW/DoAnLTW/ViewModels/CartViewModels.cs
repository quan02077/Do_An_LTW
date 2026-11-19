using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.ViewModels;

public class AddCartItemViewModel
{
    [Required]
    public int MaTui { get; set; }

    [Range(1, 99)]
    public int SoLuong { get; set; } = 1;
}

public class UpdateCartItemViewModel
{
    [Range(1, 99)]
    public int SoLuong { get; set; }
}

public record CartItemViewModel(int MaTui, string TenTui, int SoLuong, decimal DonGia, decimal ThanhTien, string? Anh)
{
    public string MaHienThi => $"TX{MaTui:D2}";
}

public record CartViewModel(IReadOnlyCollection<CartItemViewModel> Items, int TongSoLuong, decimal TongTien)
{
    // giỏ con chỉ gồm các sản phẩm khách tick chọn để thanh toán (tổng được tính lại theo phần đã chọn)
    public CartViewModel Only(IEnumerable<int> maTuis)
    {
        var selected = maTuis.ToHashSet();
        var items = Items.Where(x => selected.Contains(x.MaTui)).ToList();
        return new CartViewModel(items, items.Sum(x => x.SoLuong), items.Sum(x => x.ThanhTien));
    }
}
