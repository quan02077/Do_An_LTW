using Microsoft.Extensions.Options;

namespace DoAnLTW.Services.Chatbot;

/// <summary>
/// cổng chặn phía mình trước khi gọi AI: giữ số lượt gọi trong hạn mức miễn phí của nhà cung cấp
/// (Groq giới hạn token/phút và yêu cầu/ngày). Vượt hạn mức thì không gọi, ChatbotService dùng fallback
/// ngay lập tức, đỡ tốn một lượt gọi chắc chắn bị 429.
/// </summary>
public sealed class ChatLlmGate(IOptions<ChatbotOptions> options, TimeProvider clock)
{
    private readonly object _lock = new();
    private readonly Queue<DateTimeOffset> _recentCalls = new();
    private DateOnly _day;
    private int _usedToday;

    // true = được phép gọi AI (và đã ghi nhận một lượt)
    public bool TryAcquire()
    {
        var o = options.Value;
        var now = clock.GetLocalNow();

        lock (_lock)
        {
            var today = DateOnly.FromDateTime(now.DateTime);
            if (today != _day)
            {
                _day = today;
                _usedToday = 0;
            }

            while (_recentCalls.Count > 0 && now - _recentCalls.Peek() >= TimeSpan.FromMinutes(1))
            {
                _recentCalls.Dequeue();
            }

            if (_usedToday >= o.DailyLlmBudget || _recentCalls.Count >= o.GlobalLlmCallsPerMinute)
            {
                return false;
            }

            _recentCalls.Enqueue(now);
            _usedToday++;
            return true;
        }
    }
}
