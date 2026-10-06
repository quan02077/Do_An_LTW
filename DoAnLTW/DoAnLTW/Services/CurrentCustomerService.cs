using System.Security.Claims;
using DoAnLTW.Data;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Services;

/// <summary>
/// Chuyển claim NameIdentifier (maTK) từ cookie đăng nhập thành maKH.
/// Không nhận maKH từ request nên khách hàng không thể mạo danh người khác.
/// </summary>
public interface ICurrentCustomerService
{
    Task<Guid> GetRequiredCustomerIdAsync(CancellationToken cancellationToken = default);
}

public class CurrentCustomerService(IHttpContextAccessor httpContextAccessor, DbContext_TuiXach db) : ICurrentCustomerService
{
    public async Task<Guid> GetRequiredCustomerIdAsync(CancellationToken cancellationToken = default)
    {
        var accountValue = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(accountValue, out var maTk))
        {
            throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
        }

        var maKh = await db.KhachHangs
            .Where(x => x.matk == maTk)
            .Select(x => (Guid?)x.maKH)
            .SingleOrDefaultAsync(cancellationToken);

        return maKh ?? throw new UnauthorizedAccessException("Tài khoản này chưa có hồ sơ khách hàng.");
    }
}
