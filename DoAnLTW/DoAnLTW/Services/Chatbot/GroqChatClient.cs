using System.Text.Json;
using Microsoft.Extensions.Options;

namespace DoAnLTW.Services.Chatbot;

/// <summary>
/// gọi Groq (API tương thích OpenAI) với mô hình openai/gpt-oss-20b.
/// Phần gọi mạng, thử lại có backoff và đọc kết quả nằm ở lớp cha OpenAiCompatibleChatClient.
/// </summary>
public class GroqChatClient(HttpClient http, IOptions<ChatbotOptions> options, ILogger<GroqChatClient> logger)
    : OpenAiCompatibleChatClient(http, options.Value.Groq, logger)
{
    private readonly GroqOptions _groq = options.Value.Groq;

    protected override string Label => "Groq";

    public override string Name => $"groq:{_groq.Model}";

    protected override string BuildBody(IReadOnlyList<ChatMessage> messages) => JsonSerializer.Serialize(new
    {
        model = _groq.Model,
        messages = messages.Select(m => new { role = m.Role, content = m.Content }),
        temperature = _groq.Temperature,
        max_completion_tokens = _groq.MaxCompletionTokens,
        reasoning_effort = _groq.ReasoningEffort,
        include_reasoning = false,
        stream = false
    });
}
