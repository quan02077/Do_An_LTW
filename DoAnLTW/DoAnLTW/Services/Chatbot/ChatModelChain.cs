namespace DoAnLTW.Services.Chatbot;

/// <summary>
/// thử lần lượt các mô hình AI đã cấu hình khóa (thứ tự đăng ký: Groq rồi Gemma).
/// Mô hình trước ném ChatModelException (hết hạn mức, lỗi mạng...) thì chuyển sang mô hình kế tiếp;
/// hết mô hình thì ném lỗi để ChatbotService dùng gợi ý từ CSDL.
/// </summary>
public sealed class ChatModelChain(IReadOnlyList<IChatModelClient> clients, ILogger<ChatModelChain> logger) : IChatModelClient
{
    private readonly IReadOnlyList<IChatModelClient> _clients = clients;

    public string Name => string.Join(" -> ", _clients.Where(c => c.IsConfigured).Select(c => c.Name));

    public bool IsConfigured => _clients.Any(c => c.IsConfigured);

    public async Task<string> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken)
    {
        ChatModelException? last = null;

        foreach (var client in _clients.Where(c => c.IsConfigured))
        {
            try
            {
                return await client.CompleteAsync(messages, cancellationToken);
            }
            catch (ChatModelException ex)
            {
                last = ex;
                logger.LogWarning("Mô hình {Model} không trả lời được ({Reason}), thử mô hình dự phòng nếu có.", client.Name, ex.Message);
            }
        }

        throw last ?? new ChatModelException("Chưa cấu hình mô hình AI nào.");
    }
}
