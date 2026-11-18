using System.Security.Claims;
using DoAnLTW.Data;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Services;

public interface ICurrentCustomerService
{
    Task<int> GetRequiredCustomerIdAsync(CancellationToken cancellationToken = default);
    Task<int?> GetCustomerIdAsync(CancellationToken cancellationToken = default);
}

public class CurrentCustomerService(IHttpContextAccessor httpContextAccessor, DbContext_TuiXach db) : ICurrentCustomerService
{
    public async Task<int?> GetCustomerIdAsync(CancellationToken cancellationToken = default)
    {
        var accountValue = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(accountValue, out var maTk))
        {
            return null;
        }

        return await db.KhachHangs
            .Where(x => x.maTK == maTk)
            .Select(x => (int?)x.maKH)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetRequiredCustomerIdAsync(CancellationToken cancellationToken = default)
    {
        var maKh = await GetCustomerIdAsync(cancellationToken);
        return maKh ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ hoặc chưa có hồ sơ khách hàng.");
    }
}
