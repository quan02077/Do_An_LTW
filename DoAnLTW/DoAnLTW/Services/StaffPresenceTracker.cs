using DoAnLTW.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DoAnLTW.Services;

/// <summary>
/// theo dõi nhân viên có đang trực tuyến hay không, để khung chat của khách hiện chấm xanh/xám đúng thực tế.
/// "Trực tuyến" = có ít nhất một kết nối SignalR của tài khoản nhân viên đang mở (mọi trang admin đều mở kết nối này),
/// KHÔNG phải nhân viên chắc chắn đang ngồi nhìn màn hình. Chỉ lưu trong bộ nhớ của một máy chủ.
/// Khi kết nối cuối cùng đóng, đợi một khoảng ngắn (mặc định 10 giây, cấu hình Chat:StaffOfflineGraceSeconds) rồi mới báo offline,
/// để việc tải lại trang hoặc chuyển trang admin không làm chỉ báo nhấp nháy.
/// </summary>
public sealed class StaffPresenceTracker
{
    private readonly IHubContext<SupportChatHub> _hub;
    private readonly ILogger<StaffPresenceTracker> _logger;
    private readonly TimeSpan _grace;

    private readonly object _lock = new();
    private readonly HashSet<string> _connections = new();
    private bool _published;                 // trạng thái đã báo cho khách
    private CancellationTokenSource? _graceCts;

    public StaffPresenceTracker(IHubContext<SupportChatHub> hub, IConfiguration config, ILogger<StaffPresenceTracker> logger)
    {
        _hub = hub;
        _logger = logger;
        var seconds = int.TryParse(config["Chat:StaffOfflineGraceSeconds"], out var s) && s >= 0 ? s : 10;
        _grace = TimeSpan.FromSeconds(seconds);
    }

    // trạng thái hiện tại (đã tính khoảng chờ) để báo cho khách vừa mở chat
    public bool IsOnline
    {
        get { lock (_lock) return _published; }
    }

    public void StaffConnected(string connectionId)
    {
        bool becameOnline = false;
        lock (_lock)
        {
            _connections.Add(connectionId);
            _graceCts?.Cancel();             // kết nối lại trong thời gian chờ: không báo offline
            _graceCts = null;
            if (!_published)
            {
                _published = true;
                becameOnline = true;
            }
        }

        if (becameOnline) _ = PublishAsync(true);
    }

    public void StaffDisconnected(string connectionId)
    {
        CancellationToken token;
        lock (_lock)
        {
            _connections.Remove(connectionId);
            if (_connections.Count > 0 || !_published) return;

            _graceCts?.Cancel();
            _graceCts = new CancellationTokenSource();
            token = _graceCts.Token;
        }

        _ = WaitThenGoOfflineAsync(token);
    }

    private async Task WaitThenGoOfflineAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(_grace, token);
        }
        catch (OperationCanceledException)
        {
            return;                          // có nhân viên kết nối lại trong lúc chờ
        }

        bool becameOffline = false;
        lock (_lock)
        {
            if (_connections.Count == 0 && _published)
            {
                _published = false;
                becameOffline = true;
            }
        }

        if (becameOffline) await PublishAsync(false);
    }

    private async Task PublishAsync(bool online)
    {
        try
        {
            await _hub.Clients.Group(SupportChatHub.CustomersGroup).SendAsync("staffPresence", online);
        }
        catch (Exception ex)
        {
            // lỗi gửi chỉ làm chỉ báo chậm cập nhật, không được ảnh hưởng nghiệp vụ khác
            _logger.LogWarning(ex, "Không gửi được trạng thái trực tuyến của nhân viên tới khách.");
        }
    }
}
