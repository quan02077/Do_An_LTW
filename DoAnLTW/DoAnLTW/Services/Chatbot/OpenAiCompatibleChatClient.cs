using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace DoAnLTW.Services.Chatbot;

/// <summary>
/// phần dùng chung cho các nhà cung cấp có API kiểu OpenAI (Groq, OpenRouter...): gọi /chat/completions, thử lại, đọc kết quả.
/// Lớp con chỉ khai báo nhãn, cấu hình kết nối và phần thân yêu cầu riêng của nhà cung cấp.
/// - Thử lại có backoff cho 408/429/5xx và quá thời gian; nghe theo header retry-after nhưng không chờ quá MaxRetryAfterSeconds.
/// - Không bao giờ ghi log khóa API hoặc nội dung câu hỏi của khách (chỉ ghi mã trạng thái).
/// </summary>
public abstract class OpenAiCompatibleChatClient : IChatModelClient
{
    private static readonly HashSet<HttpStatusCode> RetryableStatuses =
    [
        HttpStatusCode.RequestTimeout, HttpStatusCode.TooManyRequests, HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout
    ];

    private readonly HttpClient _http;
    private readonly LlmProviderOptions _options;
    private readonly ILogger _logger;

    protected OpenAiCompatibleChatClient(HttpClient http, LlmProviderOptions options, ILogger logger)
    {
        _http = http;
        _options = options;
        _logger = logger;

        // thời gian chờ được quản lý riêng cho từng lần thử (CancelAfter), không dùng Timeout mặc định của HttpClient
        _http.Timeout = Timeout.InfiniteTimeSpan;
    }

    // tên nhà cung cấp dùng trong log và thông báo lỗi, ví dụ "Groq"
    protected abstract string Label { get; }

    public abstract string Name { get; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    protected abstract string BuildBody(IReadOnlyList<ChatMessage> messages);

    public async Task<string> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken)
    {
        if (!IsConfigured) throw new ChatModelException($"Chưa cấu hình khóa API cho {Label}.");

        var endpoint = new Uri(new Uri(_options.BaseUrl.TrimEnd('/') + "/"), "chat/completions");
        var body = BuildBody(messages);

        for (var attempt = 0; ; attempt++)
        {
            using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            attemptCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            TimeSpan? retryDelay = null;
            string failure;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

                using var response = await _http.SendAsync(request, attemptCts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(attemptCts.Token);
                    return ExtractContent(json);
                }

                failure = $"HTTP {(int)response.StatusCode}";
                if (!RetryableStatuses.Contains(response.StatusCode))
                {
                    // 400/401/403/404...: thử lại cũng vô ích (sai tham số, sai khóa...)
                    _logger.LogError("{Label} từ chối yêu cầu: {Failure}. Kiểm tra khóa API và tên model.", Label, failure);
                    throw new ChatModelException($"{Label} trả {failure}.");
                }

                retryDelay = ComputeDelay(response, attempt);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                failure = "quá thời gian chờ";
                retryDelay = ComputeDelay(null, attempt);
            }
            catch (HttpRequestException ex)
            {
                failure = "lỗi mạng";
                retryDelay = ComputeDelay(null, attempt);
                _logger.LogWarning(ex, "Lỗi mạng khi gọi {Label}.", Label);
            }
            catch (JsonException ex)
            {
                throw new ChatModelException($"Phản hồi của {Label} không phải JSON hợp lệ.", ex);
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or IndexOutOfRangeException)
            {
                // JSON hợp lệ nhưng thiếu choices/message/content
                throw new ChatModelException($"Phản hồi của {Label} có cấu trúc không như mong đợi.", ex);
            }

            if (attempt >= _options.MaxRetries)
            {
                _logger.LogWarning("{Label} thất bại sau {Attempts} lần thử: {Failure}.", Label, attempt + 1, failure);
                throw new ChatModelException($"{Label} thất bại: {failure}.");
            }

            if (retryDelay > TimeSpan.FromSeconds(_options.MaxRetryAfterSeconds))
            {
                // phải chờ quá lâu (ví dụ hết hạn mức token/phút): bỏ cuộc để khách nhận fallback ngay
                _logger.LogWarning("{Label} yêu cầu chờ {Delay:N0}s ({Failure}), bỏ qua và dùng fallback.", Label, retryDelay.Value.TotalSeconds, failure);
                throw new ChatModelException($"{Label} yêu cầu chờ quá lâu: {failure}.");
            }

            _logger.LogInformation("{Label} {Failure}, thử lại sau {Delay:N1}s (lần {Next}).", Label, failure, retryDelay.Value.TotalSeconds, attempt + 2);
            await Task.Delay(retryDelay.Value, cancellationToken);
        }
    }

    // chờ theo retry-after nếu có, ngược lại backoff lũy thừa (1s, 2s, 4s...) cộng nhiễu nhỏ
    private static TimeSpan ComputeDelay(HttpResponseMessage? response, int attempt)
    {
        var retryAfter = response?.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta) return delta;
        if (retryAfter?.Date is { } date) return date - DateTimeOffset.UtcNow;

        return TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 250));
    }

    private string ExtractContent(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ChatModelException($"{Label} trả về nội dung rỗng.");
        }

        return content;
    }
}
