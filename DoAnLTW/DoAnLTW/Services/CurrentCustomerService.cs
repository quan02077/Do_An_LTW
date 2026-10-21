using System.Security.Claims;
using DoAnLTW.Data;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Services;

public interface ICurrentCustomerService
{
    Task<int> GetRequiredCustomerIdAsync(CancellationToken cancellationToken = default);
}

public class CurrentCustomerService(IHttpContextAccessor httpContextAccessor, DbContext_TuiXach db) : ICurrentCustomerService
{
    public async Task<int> GetRequiredCustomerIdAsync(CancellationToken cancellationToken = default)
    {
        var accountValue = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(accountValue, out var maTk))
        {
            throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
        }

        var maKh = await db.KhachHangs
            .Where(x => x.maTK == maTk)
            .Select(x => (int?)x.maKH)
            .SingleOrDefaultAsync(cancellationToken);

        return maKh ?? throw new UnauthorizedAccessException("Tài khoản này chưa có hồ sơ khách hàng.");
    }
}
