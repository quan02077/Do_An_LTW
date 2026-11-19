using System.Security.Claims;
using DoAnLTW.Data;
using DoAnLTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Hubs;

// hub thông báo thời gian thực: chỉ gán nhóm phía server theo claim đăng nhập,
// không mở phương thức public cho client tự tham gia nhóm hay truyền mã khách.
[Authorize]
public class NotificationHub(DbContext_TuiXach db) : Hub
{
    internal const string AdminGroup = "admins";
    internal static string CustomerGroup(int maKh) => $"customer-{maKh}";

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole(Roles.Admin) == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup, Context.ConnectionAborted);
        }
        else
        {
            var accountValue = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(accountValue, out var maTk))
            {
                var maKh = await db.KhachHangs
                    .AsNoTracking()
                    .Where(x => x.maTK == maTk)
                    .Select(x => (int?)x.maKH)
                    .SingleOrDefaultAsync(Context.ConnectionAborted);

                if (maKh.HasValue)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, CustomerGroup(maKh.Value), Context.ConnectionAborted);
                }
            }
        }

        await base.OnConnectedAsync();
    }
}
