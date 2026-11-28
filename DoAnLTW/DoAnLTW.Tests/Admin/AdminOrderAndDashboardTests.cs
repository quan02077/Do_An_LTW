using DoAnLTW.Areas.Admin.Services;
using DoAnLTW.Models;
using DoAnLTW.Services;
using DoAnLTW.Tests.TestHelpers;
using Moq;
using Xunit;

namespace DoAnLTW.Tests.Admin;

public class AdminOrderAndDashboardTests
{
    private readonly Mock<INotificationService> _mockNoti = new();

    [Fact]
    public async Task AdminOrderService_UpdateStatus_ChangesOrderStatus()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var order = new DonHang
        {
            maDH = 1,
            maKH = 1,
            trangThai = OrderStatus.ChoDuyet,
            tongTien = 1_000_000,
            diaChiNhan = "123 Đường A",
            tenNguoiNhan = "Khách A",
            sdtNguoiNhan = "0900000001",
            phuongThucThanhToan = PaymentMethod.COD
        };
        db.DonHangs.Add(order);
        await db.SaveChangesAsync();

        var service = new AdminOrderService(db, _mockNoti.Object);

        // Act: duyệt đơn sang "Đang giao"
        var result = await service.UpdateStatusAsync(order.maDH, OrderStatus.DangGiao);

        // Assert
        Assert.True(result.Success);
        var updated = await db.DonHangs.FindAsync(order.maDH);
        Assert.NotNull(updated);
        Assert.Equal(OrderStatus.DangGiao, updated.trangThai);
    }

    [Fact]
    public async Task AdminDashboardService_GetStats_CalculatesTotalsCorrectly()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var now = DateTime.Now;
        db.DonHangs.AddRange(
            new DonHang
            {
                maDH = 10,
                maKH = 1,
                ngayDat = now,
                trangThai = OrderStatus.HoanTat,
                tongTien = 2_000_000,
                diaChiNhan = "A",
                tenNguoiNhan = "A",
                sdtNguoiNhan = "01",
                phuongThucThanhToan = "COD"
            },
            new DonHang
            {
                maDH = 11,
                maKH = 2,
                ngayDat = now,
                trangThai = OrderStatus.ChoDuyet,
                tongTien = 1_500_000,
                diaChiNhan = "B",
                tenNguoiNhan = "B",
                sdtNguoiNhan = "02",
                phuongThucThanhToan = "COD"
            }
        );
        await db.SaveChangesAsync();

        var service = new AdminDashboardService(db);

        // Act
        var stats = await service.GetStatsAsync(now.Year);

        // Assert: 2 orders in total, completed revenue is 2,000,000
        Assert.NotNull(stats);
        Assert.Equal(2, stats.TongDonHang);
        Assert.Equal(2_000_000, stats.TongDoanhThu);
    }
}
