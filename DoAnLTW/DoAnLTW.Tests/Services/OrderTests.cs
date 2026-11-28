using DoAnLTW.Data;
using DoAnLTW.Models;
using DoAnLTW.Services;
using DoAnLTW.Tests.TestHelpers;
using DoAnLTW.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DoAnLTW.Tests.Services;

public class OrderTests
{
    private readonly ProductPricingService _pricingService = new();
    private readonly Mock<INotificationService> _mockNoti = new();
    private readonly Mock<IOrderEmailQueue> _mockEmail = new();
    private readonly Mock<ILogger<OrderService>> _mockLogger = new();

    private OrderService CreateOrderService(DbContext_TuiXach db)
    {
        return new OrderService(db, _pricingService, _mockNoti.Object, _mockEmail.Object, _mockLogger.Object);
    }

    [Fact]
    public void OrderStatus_CanTransition_ValidatesWorkflowCorrectly()
    {
        // ChoDuyet can move to DangGiao or DaHuy
        Assert.True(OrderStatus.CanTransition(OrderStatus.ChoDuyet, OrderStatus.DangGiao));
        Assert.True(OrderStatus.CanTransition(OrderStatus.ChoDuyet, OrderStatus.DaHuy));
        Assert.False(OrderStatus.CanTransition(OrderStatus.ChoDuyet, OrderStatus.HoanTat));

        // DangGiao can move to HoanTat or DaHuy
        Assert.True(OrderStatus.CanTransition(OrderStatus.DangGiao, OrderStatus.HoanTat));
        Assert.True(OrderStatus.CanTransition(OrderStatus.DangGiao, OrderStatus.DaHuy));
        Assert.False(OrderStatus.CanTransition(OrderStatus.DangGiao, OrderStatus.ChoDuyet));

        // HoanTat and DaHuy are terminal states
        Assert.False(OrderStatus.CanTransition(OrderStatus.HoanTat, OrderStatus.DaHuy));
        Assert.False(OrderStatus.CanTransition(OrderStatus.DaHuy, OrderStatus.ChoDuyet));
    }

    [Fact]
    public void PaymentMethod_IsValid_AcceptsOnlyConfiguredMethods()
    {
        Assert.True(PaymentMethod.IsValid(PaymentMethod.COD));
        Assert.True(PaymentMethod.IsValid(PaymentMethod.ChuyenKhoan));
        Assert.False(PaymentMethod.IsValid("Bitcoin"));
        Assert.False(PaymentMethod.IsValid(null));
    }

    [Fact]
    public async Task IsVipAsync_CustomerIsVip_ReturnsTrue()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var customer = new KhachHang { maKH = 101, hoTen = "Khách Hàng VIP", isVip = true };
        db.KhachHangs.Add(customer);
        await db.SaveChangesAsync();

        var service = CreateOrderService(db);

        // Act
        var isVip = await service.IsVipAsync(customer.maKH);

        // Assert
        Assert.True(isVip);
    }

    [Fact]
    public async Task IsVipAsync_CustomerIsRegular_ReturnsFalse()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var customer = new KhachHang { maKH = 102, hoTen = "Khách Hàng Thường", isVip = false };
        db.KhachHangs.Add(customer);
        await db.SaveChangesAsync();

        var service = CreateOrderService(db);

        // Act
        var isVip = await service.IsVipAsync(customer.maKH);

        // Assert
        Assert.False(isVip);
    }

    [Fact]
    public async Task CheckoutAsync_RegularCustomer_CalculatesNormalTotal()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var customer = new KhachHang { maKH = 201, hoTen = "Khach Thuong", isVip = false, email = "test@example.com" };
        var product = new TuiXach { maTui = 10, tenTui = "Túi Tote Da", donGia = 1_000_000, soLuong = 10 };
        var cart = new GioHang { maKH = customer.maKH, maGioHang = 201 };
        cart.ChiTietGioHangs.Add(new ChiTietGioHang { maGioHang = cart.maGioHang, maTui = product.maTui, soLuong = 2 });

        db.KhachHangs.Add(customer);
        db.TuiXachs.Add(product);
        db.GioHangs.Add(cart);
        await db.SaveChangesAsync();

        var service = CreateOrderService(db);
        var checkoutReq = new CheckoutViewModel
        {
            DiaChiNhan = "123 Le Loi, Q1",
            TenNguoiNhan = "Khach Thuong",
            SdtNguoiNhan = "0901234567",
            PhuongThucThanhToan = PaymentMethod.COD,
            SelectedMaTui = [product.maTui]
        };

        // Act
        var order = await service.CheckoutAsync(customer.maKH, checkoutReq);

        // Assert: 2 x 1,000,000 = 2,000,000 (No VIP discount)
        Assert.NotNull(order);
        Assert.Equal(2_000_000, order.TongTien);
        Assert.Equal(OrderStatus.ChoDuyet, order.TrangThai);
    }

    [Fact]
    public async Task CheckoutAsync_VipCustomer_AppliesFivePercentDiscount()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var customer = new KhachHang { maKH = 202, hoTen = "Khach VIP", isVip = true, email = "vip@example.com" };
        var product = new TuiXach { maTui = 11, tenTui = "Túi Xách Sang Trọng", donGia = 2_000_000, soLuong = 5 };
        var cart = new GioHang { maKH = customer.maKH, maGioHang = 202 };
        cart.ChiTietGioHangs.Add(new ChiTietGioHang { maGioHang = cart.maGioHang, maTui = product.maTui, soLuong = 1 });

        db.KhachHangs.Add(customer);
        db.TuiXachs.Add(product);
        db.GioHangs.Add(cart);
        await db.SaveChangesAsync();

        var service = CreateOrderService(db);
        var checkoutReq = new CheckoutViewModel
        {
            DiaChiNhan = "456 Nguyen Hue, Q1",
            TenNguoiNhan = "Khach VIP",
            SdtNguoiNhan = "0909999888",
            PhuongThucThanhToan = PaymentMethod.ChuyenKhoan,
            SelectedMaTui = [product.maTui]
        };

        // Act
        var order = await service.CheckoutAsync(customer.maKH, checkoutReq);

        // Assert: 2,000,000 * 0.95 = 1,900,000
        Assert.NotNull(order);
        Assert.Equal(1_900_000, order.TongTien);
    }

    [Fact]
    public void EWarranty_Calculation_ValidatesTwelveMonthsPeriod()
    {
        // Arrange
        var orderDate = DateTime.Now.AddMonths(-3);
        var ngayHetHan = orderDate.AddMonths(12);

        // Act
        var conHan = DateTime.Now <= ngayHetHan;
        var soNgayConLai = (ngayHetHan - DateTime.Now).Days;

        // Assert: Order placed 3 months ago still has approx 9 months left
        Assert.True(conHan);
        Assert.True(soNgayConLai > 250);

        // Expired order placed 14 months ago
        var expiredOrderDate = DateTime.Now.AddMonths(-14);
        var expiredNgayHetHan = expiredOrderDate.AddMonths(12);
        Assert.False(DateTime.Now <= expiredNgayHetHan);
    }
}
