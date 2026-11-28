using DoAnLTW.Models;
using DoAnLTW.Services;
using DoAnLTW.Tests.TestHelpers;
using DoAnLTW.ViewModels;
using Xunit;

namespace DoAnLTW.Tests.Services;

public class CartTests
{
    private readonly ProductPricingService _pricingService = new();

    [Fact]
    public async Task AddAsync_NewItem_AddsToCartSuccessfully()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var product = new TuiXach { maTui = 1, tenTui = "Túi Da Bò", donGia = 500_000, soLuong = 10 };
        var customer = new KhachHang { maKH = 1, hoTen = "Nguyen Van A" };
        db.TuiXachs.Add(product);
        db.KhachHangs.Add(customer);
        await db.SaveChangesAsync();

        var cartService = new CartService(db, _pricingService);

        // Act
        var result = await cartService.AddAsync(customer.maKH, new AddCartItemViewModel { MaTui = product.maTui, SoLuong = 2 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TongSoLuong);
        Assert.Equal(1_000_000, result.TongTien);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task AddAsync_ExistingItem_IncreasesQuantity()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var product = new TuiXach { maTui = 2, tenTui = "Túi Canvas", donGia = 300_000, soLuong = 20 };
        var customer = new KhachHang { maKH = 2, hoTen = "Tran Thi B" };
        db.TuiXachs.Add(product);
        db.KhachHangs.Add(customer);
        await db.SaveChangesAsync();

        var cartService = new CartService(db, _pricingService);

        // Act
        await cartService.AddAsync(customer.maKH, new AddCartItemViewModel { MaTui = product.maTui, SoLuong = 2 });
        var result = await cartService.AddAsync(customer.maKH, new AddCartItemViewModel { MaTui = product.maTui, SoLuong = 3 });

        // Assert
        Assert.Equal(5, result.TongSoLuong);
        Assert.Equal(1_500_000, result.TongTien);
    }

    [Fact]
    public async Task AddAsync_ExceedsStock_ThrowsInvalidOperationException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var product = new TuiXach { maTui = 3, tenTui = "Túi Hiếm", donGia = 1_000_000, soLuong = 2 };
        var customer = new KhachHang { maKH = 3, hoTen = "Le Van C" };
        db.TuiXachs.Add(product);
        db.KhachHangs.Add(customer);
        await db.SaveChangesAsync();

        var cartService = new CartService(db, _pricingService);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            cartService.AddAsync(customer.maKH, new AddCartItemViewModel { MaTui = product.maTui, SoLuong = 5 }));
    }

    [Fact]
    public async Task UpdateAsync_ValidQuantity_UpdatesItemCorrectly()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var product = new TuiXach { maTui = 4, tenTui = "Túi Du Lịch", donGia = 400_000, soLuong = 15 };
        var customer = new KhachHang { maKH = 4, hoTen = "Pham Van D" };
        db.TuiXachs.Add(product);
        db.KhachHangs.Add(customer);
        await db.SaveChangesAsync();

        var cartService = new CartService(db, _pricingService);
        await cartService.AddAsync(customer.maKH, new AddCartItemViewModel { MaTui = product.maTui, SoLuong = 1 });

        // Act
        var result = await cartService.UpdateAsync(customer.maKH, product.maTui, new UpdateCartItemViewModel { SoLuong = 4 });

        // Assert
        Assert.Equal(4, result.TongSoLuong);
        Assert.Equal(1_600_000, result.TongTien);
    }

    [Fact]
    public async Task RemoveAsync_ExistingItem_RemovesFromCart()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var product = new TuiXach { maTui = 5, tenTui = "Ví Cầm Tay", donGia = 250_000, soLuong = 10 };
        var customer = new KhachHang { maKH = 5, hoTen = "Hoang Thi E" };
        db.TuiXachs.Add(product);
        db.KhachHangs.Add(customer);
        await db.SaveChangesAsync();

        var cartService = new CartService(db, _pricingService);
        await cartService.AddAsync(customer.maKH, new AddCartItemViewModel { MaTui = product.maTui, SoLuong = 2 });

        // Act
        var result = await cartService.RemoveAsync(customer.maKH, product.maTui);

        // Assert
        Assert.Equal(0, result.TongSoLuong);
        Assert.Equal(0, result.TongTien);
        Assert.Empty(result.Items);
    }
}
