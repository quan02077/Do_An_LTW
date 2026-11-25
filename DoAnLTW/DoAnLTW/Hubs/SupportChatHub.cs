using System.Security.Claims;
using DoAnLTW.Models;
using DoAnLTW.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DoAnLTW.Hubs;

/// <summary>
/// khung chat thời gian thực giữa khách hàng và nhân viên (tài khoản vai trò Người bán).
/// Nguyên tắc bảo mật: phía server tự xác định ai đang gửi từ cookie đăng nhập, client KHÔNG truyền mã khách của chính mình;
/// nhóm (group) chỉ được gán trong OnConnectedAsync, không có phương thức cho client tự tham gia nhóm;
/// các hàm của nhân viên khóa bằng [Authorize(Roles)]; mọi tin đều được kiểm tra độ dài, ký tự và giới hạn tốc độ.
/// </summary>
[Authorize]
public class SupportChatHub(ISupportChatService service, SupportChatRateGate gate, ILogger<SupportChatHub> logger) : Hub
{
    internal const string StaffGroup = "support-staff";
    internal static string CustomerGroup(int maKh) => $"support-customer-{maKh}";

    private const string MaKhKey = "maKh";

    private bool IsStaff => Context.User?.IsInRole(Roles.Admin) == true;

    private int AccountId =>
        int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : throw new HubException("Phiên đăng nhập không hợp lệ.");

    public override async Task OnConnectedAsync()
    {
        if (IsStaff)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, StaffGroup, Context.ConnectionAborted);
        }
        else
        {
            var maKh = int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var maTk)
                ? await service.GetCustomerIdAsync(maTk, Context.ConnectionAborted)
                : null;
            if (maKh == null)
            {
                Context.Abort();
                return;
            }

            Context.Items[MaKhKey] = maKh.Value;
            await Groups.AddToGroupAsync(Context.ConnectionId, CustomerGroup(maKh.Value), Context.ConnectionAborted);
        }

        await base.OnConnectedAsync();
    }

    // khách gửi tin cho nhân viên
    public async Task SendToStaff(string text)
    {
        var maKh = RequireCustomer();
        Throttle();

        var result = await service.CustomerSendAsync(maKh, text, Context.ConnectionAborted);
        if (!result.Success) throw new HubException(result.Error);

        var dto = result.Value!;
        await Clients.Group(CustomerGroup(maKh)).SendAsync("supportMessage", dto);   // mọi tab của chính khách
        await Clients.Group(StaffGroup).SendAsync("supportMessage", dto);
    }

    // nhân viên trả lời một khách
    [Authorize(Roles = Roles.Admin)]
    public async Task SendToCustomer(int maKh, string text)
    {
        Throttle();

        var result = await service.StaffSendAsync(AccountId, maKh, text, Context.ConnectionAborted);
        if (!result.Success) throw new HubException(result.Error);

        var dto = result.Value!;
        await Clients.Group(CustomerGroup(maKh)).SendAsync("supportMessage", dto with { TenNhanVien = null });   // khách không thấy tên tài khoản nhân viên
        await Clients.Group(StaffGroup).SendAsync("supportMessage", dto);
    }

    // khách đã mở xem các tin của nhân viên
    public async Task MarkReadByCustomer()
    {
        var maKh = RequireCustomer();
        await service.MarkReadByCustomerAsync(maKh, Context.ConnectionAborted);
        await Clients.Group(CustomerGroup(maKh)).SendAsync("supportRead");   // đồng bộ huy hiệu ở các tab khác
    }

    // nhân viên đã mở xem cuộc trò chuyện của một khách
    [Authorize(Roles = Roles.Admin)]
    public async Task MarkReadByStaff(int maKh)
    {
        await service.MarkReadByStaffAsync(maKh, Context.ConnectionAborted);
        await Clients.Group(StaffGroup).SendAsync("supportRead", maKh);
    }

    private int RequireCustomer()
    {
        if (IsStaff) throw new HubException("Nhân viên trả lời khách trong trang Hỗ trợ khách hàng.");
        return Context.Items.TryGetValue(MaKhKey, out var v) && v is int maKh ? maKh : throw new HubException("Không tìm thấy hồ sơ khách hàng.");
    }

    private void Throttle()
    {
        if (!gate.TryAcquire(AccountId))
        {
            logger.LogWarning("Tài khoản {AccountId} gửi tin quá nhanh trong khung chat nhân viên.", AccountId);
            throw new HubException("Bạn gửi hơi nhanh, vui lòng thử lại sau ít giây.");
        }
    }
}
