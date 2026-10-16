using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Data;

/// <summary>
/// Khởi tạo và kiểm tra kết nối cơ sở dữ liệu.
/// Dữ liệu sản phẩm và tài khoản đã được nạp từ file quan_ly_tui_xach.sql.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DbContext_TuiXach>();
            var count = await db.TuiXachs.CountAsync();
            Console.WriteLine($"[DbInitializer] Kết nối CSDL QuanLyTuiXach thành công. Số lượng túi xách: {count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Lỗi kết nối CSDL: {ex.Message}");
        }
    }
}
