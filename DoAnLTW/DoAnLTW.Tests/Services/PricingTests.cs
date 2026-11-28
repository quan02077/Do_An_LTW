using DoAnLTW.Models;
using DoAnLTW.Services;
using Xunit;

namespace DoAnLTW.Tests.Services;

public class PricingTests
{
    private readonly ProductPricingService _pricingService = new();

    [Fact]
    public void GetPrice_WithoutPromotion_ReturnsFullPrice()
    {
        // Arrange
        var product = new TuiXach { maTui = 1, donGia = 500_000, KhuyenMai = null };

        // Act
        var price = _pricingService.GetPrice(product);

        // Assert
        Assert.Equal(500_000, price.DonGiaGoc);
        Assert.Equal(500_000, price.DonGiaApDung);
        Assert.Equal(0, price.SoTienGiam);
    }

    [Fact]
    public void GetPrice_WithActivePromotion_AppliesPercentageDiscount()
    {
        // Arrange: 20% discount on 1,000,000 item
        var now = DateTime.Now;
        var km = new KhuyenMai
        {
            maKM = 1,
            trangThai = true,
            phanTramGiam = 20,
            ngayBatDau = now.AddDays(-1),
            ngayKetThuc = now.AddDays(5)
        };
        var product = new TuiXach { maTui = 2, donGia = 1_000_000, KhuyenMai = km, maKM = 1 };

        // Act
        var price = _pricingService.GetPrice(product, now);

        // Assert: 1,000,000 - 200,000 = 800,000
        Assert.Equal(1_000_000, price.DonGiaGoc);
        Assert.Equal(800_000, price.DonGiaApDung);
        Assert.Equal(200_000, price.SoTienGiam);
    }
}
