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

public class GroqOptions
{
    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";
    public string Model { get; set; } = "openai/gpt-oss-20b";

    // gpt-oss-20b chỉ nhận low | medium | high (giá trị khác Groq trả 400)
    public string ReasoningEffort { get; set; } = "low";
    public int MaxCompletionTokens { get; set; } = 700;
    public double Temperature { get; set; } = 0.3;

    public int TimeoutSeconds { get; set; } = 20;     // mỗi lần gọi
    public int MaxRetries { get; set; } = 2;          // thử lại cho 429/5xx/timeout
    public int MaxRetryAfterSeconds { get; set; } = 4; // chờ lâu hơn mức này thì bỏ cuộc, dùng fallback
}
