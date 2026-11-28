using DoAnLTW.Models;
using DoAnLTW.Services;
using DoAnLTW.Tests.TestHelpers;
using DoAnLTW.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace DoAnLTW.Tests.Services;

public class AccountTests
{
    private readonly PasswordService _passwordService = new();
    private readonly Mock<IEmailService> _mockEmail = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    private AccountService CreateService(Data.DbContext_TuiXach db) =>
        new(db, _passwordService, _mockEmail.Object, _cache);

    [Fact]
    public async Task ValidateCredentials_CorrectPassword_ReturnsAccount()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var account = new TaiKhoan { maTK = 1, tenDangNhap = "user1", vaiTro = Roles.Customer };
        account.matKhau = _passwordService.Hash(account, "Password123!");
        db.TaiKhoans.Add(account);
        await db.SaveChangesAsync();

        var service = CreateService(db);

        // Act
        var result = await service.ValidateCredentialsAsync("user1", "Password123!");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user1", result.tenDangNhap);
    }

    [Fact]
    public async Task ValidateCredentials_WrongPassword_ReturnsNull()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var account = new TaiKhoan { maTK = 2, tenDangNhap = "user2", vaiTro = Roles.Customer };
        account.matKhau = _passwordService.Hash(account, "Password123!");
        db.TaiKhoans.Add(account);
        await db.SaveChangesAsync();

        var service = CreateService(db);

        // Act
        var result = await service.ValidateCredentialsAsync("user2", "WrongPass");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Register_NewUser_CreatesAccountSuccessfully()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var service = CreateService(db);
        var regModel = new RegisterViewModel
        {
            TenDangNhap = "newuser",
            Email = "newuser@example.com",
            MatKhau = "Secret123!",
            XacNhanMatKhau = "Secret123!",
            HoTen = "Nguyen Van New"
        };

        // Act
        var result = await service.RegisterAsync(regModel);

        // Assert
        Assert.True(result.Success);
        var savedAccount = db.TaiKhoans.FirstOrDefault(x => x.tenDangNhap == "newuser");
        Assert.NotNull(savedAccount);
        Assert.Equal(Roles.Customer, savedAccount.vaiTro);
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsFailure()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        db.TaiKhoans.Add(new TaiKhoan { maTK = 3, tenDangNhap = "existinguser", vaiTro = Roles.Customer, matKhau = "dummy" });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var regModel = new RegisterViewModel
        {
            TenDangNhap = "existinguser",
            Email = "different@example.com",
            MatKhau = "Secret123!",
            XacNhanMatKhau = "Secret123!",
            HoTen = "Test User"
        };

        // Act
        var result = await service.RegisterAsync(regModel);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("đã được sử dụng", result.Error);
    }
}
