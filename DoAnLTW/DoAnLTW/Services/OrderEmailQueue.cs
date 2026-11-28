using System.Threading.Channels;
using DoAnLTW.ViewModels;

namespace DoAnLTW.Services;

/// <summary>hàng đợi gửi email xác nhận đơn: OrderService chỉ xếp thư vào hàng đợi (không chờ), việc gửi SMTP chạy nền.</summary>
public interface IOrderEmailQueue
{
    // false khi hàng đợi đầy (bỏ thư, đơn hàng không bị ảnh hưởng)
    bool TryEnqueue(string to, string name, OrderViewModel order);
}

public sealed class OrderEmailQueue(IEmailService emailService, ILogger<OrderEmailQueue> logger) : BackgroundService, IOrderEmailQueue
{
    private sealed record Job(string To, string Name, OrderViewModel Order);

    // tối đa 100 thư chờ; đầy thì bỏ thư mới thay vì làm treo luồng đặt hàng
    private readonly Channel<Job> _channel = Channel.CreateBounded<Job>(new BoundedChannelOptions(100)
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
                timeout.CancelAfter(TimeSpan.FromSeconds(30));
                await emailService.SendOrdersConfirmationAsync(job.To, job.Name, job.Order, timeout.Token);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // không ghi địa chỉ email của khách vào log; đơn hàng đã lưu thành công, chỉ thư là không gửi được
                logger.LogWarning(ex, "Không gửi được mail xác nhận đơn {MaDonHang}.", job.Order.MaHienThi);
            }
        }
    }
}
