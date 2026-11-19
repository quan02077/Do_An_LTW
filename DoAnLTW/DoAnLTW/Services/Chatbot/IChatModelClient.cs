namespace DoAnLTW.Services.Chatbot;

public record ChatMessage(string Role, string Content);

/// <summary>lỗi khi gọi mô hình AI (hết hạn mức, quá thời gian, phản hồi sai...). ChatbotService bắt lỗi này và dùng fallback.</summary>
public class ChatModelException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>
/// giao diện chung cho mô hình AI. Mỗi nhà cung cấp (Groq, Gemma...) là một cài đặt riêng,
/// thêm hay đổi nhà cung cấp không phải sửa ChatbotService.
/// </summary>
public interface IChatModelClient
{
    string Name { get; }

    // false khi chưa có khóa API: chatbot vẫn chạy bằng gợi ý từ cơ sở dữ liệu (fallback)
    bool IsConfigured { get; }

    Task<string> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken);
}
