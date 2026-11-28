using DoAnLTW.Areas.Admin.Services;
using DoAnLTW.Models;
using DoAnLTW.Tests.TestHelpers;
using DoAnLTW.ViewModels;
using Xunit;

namespace DoAnLTW.Tests.Admin;

public class AdminCategoryTests
{
    [Fact]
    public async Task AddLoaiTuiAsync_ValidCategory_SavesToDatabase()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var service = new CategoryService(db);
        var newCategory = new TypeViewModel { TenLoai = "Balo Thời Trang" };

        // Act
        var success = await service.AddLoaiTuiAsync(newCategory);

        // Assert
        Assert.True(success);
        var saved = db.LoaiTuis.FirstOrDefault(x => x.tenLoai == "Balo Thời Trang");
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task UpdateLoaiTuiAsync_ExistingCategory_UpdatesName()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var category = new LoaiTui { maLoai = 1, tenLoai = "Túi Xách Tay" };
        db.LoaiTuis.Add(category);
        await db.SaveChangesAsync();

        var service = new CategoryService(db);
        var updateModel = new TypeViewModel { TenLoai = "Túi Xách Tay Cao Cấp" };

        // Act
        var success = await service.UpdateLoaiTuiAsync(category.maLoai, updateModel);

        // Assert
        Assert.True(success);
        var updated = await service.GetLoaiTuiByIdAsync(category.maLoai);
        Assert.NotNull(updated);
        Assert.Equal("Túi Xách Tay Cao Cấp", updated.TenLoai);
    }

    [Fact]
    public async Task DeleteLoaiTuiAsync_NoAssociatedProducts_DeletesSuccessfully()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var category = new LoaiTui { maLoai = 2, tenLoai = "Túi Hết Mốt" };
        db.LoaiTuis.Add(category);
        await db.SaveChangesAsync();

        var service = new CategoryService(db);

        // Act
        var success = await service.DeleteLoaiTuiAsync(category.maLoai);

        // Assert
        Assert.True(success);
        var check = await service.GetLoaiTuiByIdAsync(category.maLoai);
        Assert.Null(check);
    }

    [Fact]
    public async Task AddThuongHieuAsync_ValidBrand_SavesToDatabase()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var service = new CategoryService(db);
        var newBrand = new BrandViewModel { TenThuongHieu = "Gucci" };

        // Act
        var success = await service.AddThuongHieuAsync(newBrand);

        // Assert
        Assert.True(success);
        var saved = db.ThuongHieus.FirstOrDefault(x => x.tenThuongHieu == "Gucci");
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task UpdateThuongHieuAsync_ExistingBrand_UpdatesName()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var brand = new ThuongHieu { maThuongHieu = 1, tenThuongHieu = "Channel" };
        db.ThuongHieus.Add(brand);
        await db.SaveChangesAsync();

        var service = new CategoryService(db);
        var updateModel = new BrandViewModel { TenThuongHieu = "Chanel" };

        // Act
        var success = await service.UpdateThuongHieuAsync(brand.maThuongHieu, updateModel);

        // Assert
        Assert.True(success);
        var updated = await service.GetThuongHieuByIdAsync(brand.maThuongHieu);
        Assert.NotNull(updated);
        Assert.Equal("Chanel", updated.TenThuongHieu);
    }

    [Fact]
    public async Task DeleteThuongHieuAsync_DeletesSuccessfully()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var brand = new ThuongHieu { maThuongHieu = 2, tenThuongHieu = "Old Brand" };
        db.ThuongHieus.Add(brand);
        await db.SaveChangesAsync();

        var service = new CategoryService(db);

        // Act
        var success = await service.DeleteThuongHieuAsync(brand.maThuongHieu);

        // Assert
        Assert.True(success);
        var check = await service.GetThuongHieuByIdAsync(brand.maThuongHieu);
        Assert.Null(check);
    }
}
