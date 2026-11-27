using System.Threading.Channels;
using DoAnLTW.Constants;
using DoAnLTW.ViewModels;

namespace DoAnLTW.Services;

/// <summary>
/// Hàng đợi gửi email xác nhận đơn hàng:
/// Tách rời bước đặt hàng (Checkout) khỏi quá trình gửi SMTP tốn thời gian.
/// OrderService chỉ việc đưa thư vào hàng đợi (TryEnqueue), tiến trình nền sẽ xử lý gửi thư.
/// </summary>
public interface IOrderEmailQueue
{
    /// <summary>
    /// Đẩy thông tin đơn hàng vào hàng đợi gửi thư ngầm.
    /// </summary>
    /// <param name="to">Email người nhận</param>
    /// <param name="name">Tên khách hàng</param>
    /// <param name="order">Dữ liệu chi tiết đơn hàng</param>
    /// <returns>True nếu xếp hàng thành công; False nếu hàng đợi quá tải (bỏ qua êm)</returns>
    bool TryEnqueue(string to, string name, OrderViewModel order);
}

/// <summary>
/// Dịch vụ nền (BackgroundService) quản lý hàng đợi Producer-Consumer dùng System.Threading.Channels.
/// </summary>
public sealed class OrderEmailQueue(IEmailService emailService, ILogger<OrderEmailQueue> logger) : BackgroundService, IOrderEmailQueue
{
    private sealed record Job(string To, string Name, OrderViewModel Order);

    // Sử dụng hằng số đã chuẩn hóa thay vì magic number 100
    private readonly Channel<Job> _channel = Channel.CreateBounded<Job>(new BoundedChannelOptions(AppConstants.Email.QueueCapacity)
    {
        FullMode = BoundedChannelFullMode.DropWrite,
        SingleReader = true
    });

    public bool TryEnqueue(string to, string name, OrderViewModel order) => _channel.Writer.TryWrite(new Job(to, name, order));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(AppConstants.Email.JobTimeoutSeconds));
                await emailService.SendOrdersConfirmationAsync(job.To, job.Name, job.Order, timeout.Token);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Không ghi địa chỉ email của khách vào log; đơn hàng đã lưu thành công, chỉ thư là không gửi được
                logger.LogWarning(ex, "Không gửi được mail xác nhận đơn {MaDonHang}.", job.Order.MaHienThi);
            }
        }
    }
}
