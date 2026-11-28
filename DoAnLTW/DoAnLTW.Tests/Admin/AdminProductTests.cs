using DoAnLTW.Areas.Admin.Services;
using DoAnLTW.Models;
using DoAnLTW.Tests.TestHelpers;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace DoAnLTW.Tests.Admin;

public class AdminProductTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnv = new();

    public AdminProductTests()
    {
        _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProductsAndFiltersByKeyword()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var category = new LoaiTui { maLoai = 1, tenLoai = "Túi Da" };
        var brand = new ThuongHieu { maThuongHieu = 1, tenThuongHieu = "Chanel" };
        db.LoaiTuis.Add(category);
        db.ThuongHieus.Add(brand);

        db.TuiXachs.AddRange(
            new TuiXach { maTui = 1, tenTui = "Túi Da Cá Sấu", donGia = 1_500_000, soLuong = 10, maLoai = 1, maThuongHieu = 1 },
            new TuiXach { maTui = 2, tenTui = "Balo Du Lịch", donGia = 800_000, soLuong = 5, maLoai = 1, maThuongHieu = 1 }
        );
        await db.SaveChangesAsync();

        var service = new ProductService(db, _mockEnv.Object);

        // Act
        var all = await service.GetAllAsync(null);
        var searchResult = await service.GetAllAsync("Cá Sấu");

        // Assert
        Assert.Equal(2, all.SanPhams.Count);
        Assert.Single(searchResult.SanPhams);
        Assert.Equal("Túi Da Cá Sấu", searchResult.SanPhams[0].TenTui);
    }

    [Fact]
    public async Task AddAsync_ValidProduct_SavesToDatabase()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var category = new LoaiTui { maLoai = 1, tenLoai = "Túi Nữ" };
        var brand = new ThuongHieu { maThuongHieu = 1, tenThuongHieu = "Dior" };
        db.LoaiTuis.Add(category);
        db.ThuongHieus.Add(brand);
        await db.SaveChangesAsync();

        var service = new ProductService(db, _mockEnv.Object);
        var form = new ProductFormViewModel
        {
            TenTui = "Túi Dior Saddle",
            MaLoai = category.maLoai,
            MaThuongHieu = brand.maThuongHieu,
            DonGia = 35_000_000,
            SoLuong = 3,
            MoTa = "Túi xách cao cấp chính hãng Dior"
        };

        // Act
        var result = await service.AddAsync(form);

        // Assert
        Assert.True(result.Success);
        var saved = db.TuiXachs.FirstOrDefault(x => x.tenTui == "Túi Dior Saddle");
        Assert.NotNull(saved);
        Assert.Equal(35_000_000, saved.donGia);
    }

    [Fact]
    public async Task UpdateAsync_ExistingProduct_UpdatesFields()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var category = new LoaiTui { maLoai = 2, tenLoai = "Ví" };
        var brand = new ThuongHieu { maThuongHieu = 2, tenThuongHieu = "LV" };
        var product = new TuiXach { maTui = 10, tenTui = "Ví LV Cũ", donGia = 5_000_000, soLuong = 10, maLoai = 2, maThuongHieu = 2 };

        db.LoaiTuis.Add(category);
        db.ThuongHieus.Add(brand);
        db.TuiXachs.Add(product);
        await db.SaveChangesAsync();

        var service = new ProductService(db, _mockEnv.Object);
        var updateForm = new ProductFormViewModel
        {
            TenTui = "Ví LV Mới",
            MaLoai = 2,
            MaThuongHieu = 2,
            DonGia = 6_500_000,
            SoLuong = 8,
            MoTa = "Cập nhật mô tả mới"
        };

        // Act
        var result = await service.UpdateAsync(product.maTui, updateForm);

        // Assert
        Assert.True(result.Success);
        var updated = await service.GetByIdAsync(product.maTui);
        Assert.NotNull(updated);
        Assert.Equal("Ví LV Mới", updated.TenTui);
        Assert.Equal(6_500_000, updated.DonGia);
        Assert.Equal(8, updated.SoLuong);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProductWithoutOrders_DeletesSuccessfully()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var category = new LoaiTui { maLoai = 3, tenLoai = "Khác" };
        var brand = new ThuongHieu { maThuongHieu = 3, tenThuongHieu = "Khác" };
        var product = new TuiXach { maTui = 99, tenTui = "Túi Xóa Thử", donGia = 100_000, soLuong = 1, maLoai = 3, maThuongHieu = 3 };

        db.LoaiTuis.Add(category);
        db.ThuongHieus.Add(brand);
        db.TuiXachs.Add(product);
        await db.SaveChangesAsync();

        var service = new ProductService(db, _mockEnv.Object);

        // Act
        var result = await service.DeleteAsync(product.maTui);

        // Assert
        Assert.True(result.Success);
        var check = await service.GetByIdAsync(product.maTui);
        Assert.Null(check);
    }
}
