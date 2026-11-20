namespace DoAnLTW.Services.Chatbot;

/// <summary>
/// cấu hình chatbot, đọc từ mục "Chatbot" trong appsettings.json.
/// khóa API KHÔNG nằm trong appsettings: đặt bằng `dotnet user-secrets set "Chatbot:Groq:ApiKey" "..."`
/// hoặc biến môi trường Chatbot__Groq__ApiKey.
/// </summary>
public class ChatbotOptions
{
    public const string SectionName = "Chatbot";

    public GroqOptions Groq { get; set; } = new();
    public OpenRouterOptions OpenRouter { get; set; } = new();

    // giới hạn đầu vào (client gửi lên, không tin)
    public int MaxMessageChars { get; set; } = 500;
    public int MaxHistoryAccepted { get; set; } = 6;     // số tin lịch sử server chấp nhận
    public int MaxHistoryToModel { get; set; } = 4;      // số tin thật sự gửi cho AI (tiết kiệm token)

    // truy xuất sản phẩm đưa vào ngữ cảnh
    public int MaxProducts { get; set; } = 5;
    public int MaxContextChars { get; set; } = 2000;
    public int MaxReplyChars { get; set; } = 1200;

    // chống lạm dụng và giữ hạn mức miễn phí của Groq (TPM rất thấp)
    public int PerSessionPerMinute { get; set; } = 6;       // mỗi người dùng/IP
    public int GlobalLlmCallsPerMinute { get; set; } = 4;   // toàn hệ thống
    public int DailyLlmBudget { get; set; } = 800;          // dưới mức 1.000 yêu cầu/ngày
}

/// <summary>cấu hình kết nối chung của mọi nhà cung cấp AI kiểu OpenAI.</summary>
public abstract class LlmProviderOptions
{
    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 20;     // mỗi lần gọi
    public int MaxRetries { get; set; } = 2;          // thử lại cho 429/5xx/timeout
    public int MaxRetryAfterSeconds { get; set; } = 4; // chờ lâu hơn mức này thì bỏ cuộc, chuyển mô hình dự phòng/fallback
}

public class GroqOptions : LlmProviderOptions
{
    public GroqOptions() => BaseUrl = "https://api.groq.com/openai/v1/";

    public string Model { get; set; } = "openai/gpt-oss-20b";

    // gpt-oss-20b chỉ nhận low | medium | high (giá trị khác Groq trả 400)
    public string ReasoningEffort { get; set; } = "low";
    public int MaxCompletionTokens { get; set; } = 700;
    public double Temperature { get; set; } = 0.3;
}

/// <summary>Gemma qua OpenRouter, dự phòng cho Groq. Khóa: Chatbot:OpenRouter:ApiKey / biến Chatbot__OpenRouter__ApiKey.</summary>
public class OpenRouterOptions : LlmProviderOptions
{
    public OpenRouterOptions() { BaseUrl = "https://openrouter.ai/api/v1/"; MaxRetries = 1; }

    // bản ":free" miễn phí nhưng hạn mức thấp; đổi sang "google/gemma-4-31b-it" (trả phí rất rẻ) nếu cần ổn định hơn
    public string Model { get; set; } = "google/gemma-4-31b-it:free";
    public int MaxTokens { get; set; } = 700;
    public double Temperature { get; set; } = 0.3;
}
