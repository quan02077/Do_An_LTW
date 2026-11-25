using System.Security.Claims;
using DoAnLTW.Models;
using DoAnLTW.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

/// <summary>
/// đọc lịch sử và số tin chưa đọc của khung chat nhân viên (chỉ GET, gửi tin đi qua SupportChatHub để có thời gian thực).
/// Khách chỉ đọc được cuộc trò chuyện của chính mình (mã khách lấy từ cookie, không nhận từ client);
/// các đường dẫn của nhân viên khóa bằng vai trò Người bán.
/// </summary>
[Route("api/support-chat")]
[Authorize]
public class SupportChatApiController(ISupportChatService service) : ControllerBase
{
    private const int HistoryTake = 60;

    // GET /api/support-chat/me: lịch sử của khách đang đăng nhập + số tin nhân viên chưa đọc
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (User.IsInRole(Roles.Admin)) return Forbid();

        var maKh = await CurrentCustomerIdAsync(ct);
        if (maKh == null) return NotFound(new { error = "Không tìm thấy hồ sơ khách hàng." });

        var history = await service.GetCustomerHistoryAsync(maKh.Value, HistoryTake, ct);
        var unread = await service.CountUnreadForCustomerAsync(maKh.Value, ct);
        return Ok(new { history, unread });
    }

    // GET /api/support-chat/unread: tổng số tin chưa đọc (khách: của chính mình; nhân viên: toàn bộ cửa hàng)
    [HttpGet("unread")]
    public async Task<IActionResult> Unread(CancellationToken ct)
    {
        if (User.IsInRole(Roles.Admin)) return Ok(new { unread = await service.CountUnreadForStaffAsync(ct) });

        var maKh = await CurrentCustomerIdAsync(ct);
        return Ok(new { unread = maKh == null ? 0 : await service.CountUnreadForCustomerAsync(maKh.Value, ct) });
    }

    // GET /api/support-chat/conversations: hộp thư của nhân viên
    [HttpGet("conversations")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Conversations(CancellationToken ct) =>
        Ok(await service.ListConversationsAsync(ct));

    // GET /api/support-chat/conversations/{maKh}: nội dung cuộc trò chuyện với một khách
    [HttpGet("conversations/{maKh:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Conversation(int maKh, CancellationToken ct)
    {
        var messages = await service.GetConversationAsync(maKh, HistoryTake, ct);
        return messages == null ? NotFound(new { error = "Không tìm thấy khách hàng này." }) : Ok(messages);
    }

    private async Task<int?> CurrentCustomerIdAsync(CancellationToken ct) =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var maTk) ? await service.GetCustomerIdAsync(maTk, ct) : null;
}
