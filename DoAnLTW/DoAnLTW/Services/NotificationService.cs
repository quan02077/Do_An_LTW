using DoAnLTW.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DoAnLTW.Services;

public record OrderNotification(
    int MaDh, string MaHienThi, int MaKh, string TenNguoiNhan,
    decimal TongTien, DateTime NgayDat, string TrangThai, string? TrangThaiCu = null);

public interface INotificationService
{
    Task NotifyOrderCreatedAsync(OrderNotification order, CancellationToken ct = default);
    Task NotifyOrderCancelledByCustomerAsync(OrderNotification order, CancellationToken ct = default);
    Task NotifyOrderStatusChangedAsync(OrderNotification order, CancellationToken ct = default);
}

public class NotificationService(
    IHubContext<NotificationHub> hubContext,
    ILogger<NotificationService> logger) : INotificationService
{
    public Task NotifyOrderCreatedAsync(OrderNotification order, CancellationToken ct = default) =>
        SendSafeAsync(NotificationHub.AdminGroup, "newOrder", order, ct);

    public Task NotifyOrderCancelledByCustomerAsync(OrderNotification order, CancellationToken ct = default) =>
        SendSafeAsync(NotificationHub.AdminGroup, "orderCancelled", order, ct);

    public Task NotifyOrderStatusChangedAsync(OrderNotification order, CancellationToken ct = default) =>
        SendSafeAsync(NotificationHub.CustomerGroup(order.MaKh), "orderStatusChanged", order, ct);

    // bọc try/catch để lỗi thông báo không bao giờ ảnh hưởng nghiệp vụ chính
    private async Task SendSafeAsync(string group, string eventName, OrderNotification order, CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.Group(group).SendAsync(eventName, order, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không thể gửi thông báo SignalR {EventName} tới nhóm {Group} cho đơn {MaHienThi}.",
                eventName, group, order.MaHienThi);
        }
    }
}
