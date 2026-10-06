using DoAnLTW.Models;

namespace DoAnLTW.Services;

/// <summary>
/// Điểm dùng chung để tính giá hiển thị và giá chốt khi tạo đơn.
/// Khi phần khuyến mãi hoàn thiện, chỉ cần mở rộng service này.
/// </summary>
public interface IProductPricingService
{
    ProductPrice GetPrice(TuiXach product, DateTime? now = null);
}

public record ProductPrice(decimal DonGiaGoc, decimal DonGiaApDung, decimal SoTienGiam);

public class ProductPricingService : IProductPricingService
{
    public ProductPrice GetPrice(TuiXach product, DateTime? now = null)
    {
        var time = now ?? DateTime.Now;
        var promotion = product.KhuyenMai;
        var active = promotion is not null && promotion.trangThai
            && promotion.ngayBatDau <= time && time <= promotion.ngayKetThuc;

        if (!active)
        {
            return new ProductPrice(product.donGia, product.donGia, 0);
        }

        var percent = Math.Clamp((decimal)promotion!.phanTramGiam, 0, 100);
        var discount = Math.Round(product.donGia * percent / 100, 2, MidpointRounding.AwayFromZero);
        return new ProductPrice(product.donGia, product.donGia - discount, discount);
    }
}
