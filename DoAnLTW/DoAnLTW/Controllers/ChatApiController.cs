using DoAnLTW.Services.Chatbot;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace DoAnLTW.Controllers;

/// <summary>
/// POST /api/chat: tư vấn túi xách. CHỈ khách đã đăng nhập mới dùng được (chống bot/script đốt hạn mức AI), chưa đăng nhập
/// nhận 401. Vì xác thực bằng cookie nên bắt buộc kèm token chống giả mạo (header RequestVerificationToken, cấu hình trong
/// Program.cs). Thêm giới hạn tốc độ theo tài khoản (policy "chat") và kiểm tra đầu vào chặt. Cố ý KHÔNG dùng [ApiController]
/// để mọi lỗi đầu vào trả về cùng một dạng { "error": "..." } cho trình duyệt.
/// </summary>
[Route("api/chat")]
[Authorize]
public class ChatApiController(
    IChatbotService chatbot,
    IOptions<ChatbotOptions> options,
    ILogger<ChatApiController> logger) : ControllerBase
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("chat")]
    public async Task<IActionResult> Post([FromBody] ChatRequest? request, CancellationToken ct)
    {
        var o = options.Value;

        var message = request?.Message?.Trim();
        if (string.IsNullOrEmpty(message))
        {
            return BadRequest(new { error = "Vui lòng nhập câu hỏi." });
        }

        if (message.Length > o.MaxMessageChars)
        {
            return BadRequest(new { error = $"Câu hỏi tối đa {o.MaxMessageChars} ký tự." });
        }

        // lịch sử do trình duyệt gửi: chỉ nhận vai user/assistant (không bao giờ nhận "system"), cắt độ dài và số lượng
        var history = (request!.History ?? [])
            .Where(h => h is not null && !string.IsNullOrWhiteSpace(h.Content)
                        && (string.Equals(h.Role, "user", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(h.Role, "assistant", StringComparison.OrdinalIgnoreCase)))
            .TakeLast(o.MaxHistoryAccepted)
            .Select(h => new ChatTurn(h.Role!.ToLowerInvariant(), Truncate(h.Content!.Trim(), o.MaxMessageChars)))
            .ToList();

        try
        {
            return Ok(await chatbot.AskAsync(message, history, ct));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return new EmptyResult(); // trình duyệt đã đóng kết nối
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi không mong đợi khi xử lý /api/chat.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Hệ thống đang bận, vui lòng thử lại sau ít phút." });
        }
    }

    private static string Truncate(string text, int max) => text.Length <= max ? text : text[..max];
}
