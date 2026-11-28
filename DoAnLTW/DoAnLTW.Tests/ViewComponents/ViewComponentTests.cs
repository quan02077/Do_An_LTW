using System.Security.Claims;
using DoAnLTW.Data;
using DoAnLTW.Models;
using DoAnLTW.Services;
using DoAnLTW.Tests.TestHelpers;
using DoAnLTW.ViewComponents;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using Xunit;

namespace DoAnLTW.Tests.ViewComponents;

public class ViewComponentTests
{
    private static ViewComponentContext CreateContextWithUser(ClaimsPrincipal principal)
    {
        var httpContext = new DefaultHttpContext { User = principal };
        var viewContext = new ViewContext { HttpContext = httpContext };
        return new ViewComponentContext { ViewContext = viewContext };
    }

    [Fact]
    public async Task CartSummary_AuthenticatedUser_ReturnsItemCount()
    {
        // Arrange
        var mockCart = new Mock<ICartService>();
        var mockCustomer = new Mock<ICurrentCustomerService>();

        mockCustomer.Setup(c => c.GetCustomerIdAsync(default)).ReturnsAsync(1);
        mockCart.Setup(c => c.GetAsync(1, default)).ReturnsAsync(new CartViewModel([], 3, 500_000));

        var claims = new[] { new Claim(ClaimTypes.Name, "NguyenVanA") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var component = new CartSummaryViewComponent(mockCart.Object, mockCustomer.Object)
        {
            ViewComponentContext = CreateContextWithUser(principal)
        };

        // Act
        var result = await component.InvokeAsync();

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.Equal(3, Assert.IsType<int>(viewResult.ViewData?.Model));
    }

    [Fact]
    public async Task CartSummary_UnauthenticatedUser_ReturnsZero()
    {
        // Arrange
        var mockCart = new Mock<ICartService>();
        var mockCustomer = new Mock<ICurrentCustomerService>();

        var anonymousPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        var component = new CartSummaryViewComponent(mockCart.Object, mockCustomer.Object)
        {
            ViewComponentContext = CreateContextWithUser(anonymousPrincipal)
        };

        // Act
        var result = await component.InvokeAsync();

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.Equal(0, Assert.IsType<int>(viewResult.ViewData?.Model));
    }

    [Fact]
    public async Task VipBadge_VipCustomer_ReturnsTrue()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var customer = new KhachHang { maKH = 10, hoTen = "VIP User", isVip = true };
        db.KhachHangs.Add(customer);
        await db.SaveChangesAsync();

        var mockCustomer = new Mock<ICurrentCustomerService>();
        mockCustomer.Setup(c => c.GetCustomerIdAsync(default)).ReturnsAsync(customer.maKH);

        var claims = new[] { new Claim(ClaimTypes.Name, "VIP User") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var component = new VipBadgeViewComponent(mockCustomer.Object, db)
        {
            ViewComponentContext = CreateContextWithUser(principal)
        };

        // Act
        var result = await component.InvokeAsync();

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.True(Assert.IsType<bool>(viewResult.ViewData?.Model));
    }

    [Fact]
    public async Task VipBadge_RegularCustomer_ReturnsFalse()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var customer = new KhachHang { maKH = 11, hoTen = "Regular User", isVip = false };
        db.KhachHangs.Add(customer);
        await db.SaveChangesAsync();

        var mockCustomer = new Mock<ICurrentCustomerService>();
        mockCustomer.Setup(c => c.GetCustomerIdAsync(default)).ReturnsAsync(customer.maKH);

        var claims = new[] { new Claim(ClaimTypes.Name, "Regular User") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var component = new VipBadgeViewComponent(mockCustomer.Object, db)
        {
            ViewComponentContext = CreateContextWithUser(principal)
        };

        // Act
        var result = await component.InvokeAsync();

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.False(Assert.IsType<bool>(viewResult.ViewData?.Model));
    }

    [Fact]
    public async Task VipBadge_UnauthenticatedUser_ReturnsEmptyContent()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var mockCustomer = new Mock<ICurrentCustomerService>();

        var anonymousPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        var component = new VipBadgeViewComponent(mockCustomer.Object, db)
        {
            ViewComponentContext = CreateContextWithUser(anonymousPrincipal)
        };

        // Act
        var result = await component.InvokeAsync();

        // Assert
        var contentResult = Assert.IsType<ContentViewComponentResult>(result);
        Assert.Equal(string.Empty, contentResult.Content);
    }
}
