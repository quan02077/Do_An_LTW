using System.Collections.Concurrent;

namespace DoAnLTW.Services;

/// <summary>
/// giới hạn tốc độ gửi tin của từng tài khoản trong khung chat nhân viên (chống spam làm ngập hộp thư).
/// Trượt theo cửa sổ thời gian, lưu trong bộ nhớ (một máy chủ). Hub SignalR không đi qua middleware giới hạn của ASP.NET Core
/// nên phải tự kiểm tra ở từng phương thức gửi tin.
/// </summary>
public sealed class SupportChatRateGate(TimeProvider clock)
{
    private const int MaxMessages = 15;
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<int, Queue<DateTimeOffset>> _sent = new();

    // true = được gửi (và đã ghi nhận một lượt)
    public bool TryAcquire(int accountId)
    {
        var now = clock.GetUtcNow();
        var queue = _sent.GetOrAdd(accountId, _ => new Queue<DateTimeOffset>());
        lock (queue)
        {
            while (queue.Count > 0 && now - queue.Peek() >= Window) queue.Dequeue();
            if (queue.Count >= MaxMessages) return false;
            queue.Enqueue(now);
            return true;
        }
    }
}
