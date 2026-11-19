using System.Text.Json;
using Microsoft.Extensions.Options;

namespace DoAnLTW.Services.Chatbot;

/// <summary>
/// gọi OpenRouter (API tương thích OpenAI) với mô hình Gemma, dùng làm dự phòng khi Groq lỗi hoặc hết hạn mức.
/// Không gửi thêm thông tin định danh nào (không có header Referer/Title).
/// </summary>
public class OpenRouterChatClient(HttpClient http, IOptions<ChatbotOptions> options, ILogger<OpenRouterChatClient> logger)
    : OpenAiCompatibleChatClient(http, options.Value.OpenRouter, logger)
{
    private readonly OpenRouterOptions _router = options.Value.OpenRouter;

    protected override string Label => "OpenRouter";

    public override string Name => $"openrouter:{_router.Model}";

    protected override string BuildBody(IReadOnlyList<ChatMessage> messages) => JsonSerializer.Serialize(new
    {
        model = _router.Model,
        messages = messages.Select(m => new { role = m.Role, content = m.Content }),
        temperature = _router.Temperature,
        max_tokens = _router.MaxTokens,
        // tắt chế độ "suy nghĩ" để toàn bộ token dành cho câu trả lời (tránh content rỗng khi hết token)
        reasoning = new { enabled = false },
        stream = false
    });
}
