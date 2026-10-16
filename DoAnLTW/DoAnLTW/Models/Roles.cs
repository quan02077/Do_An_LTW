namespace DoAnLTW.Models;

/// <summary>
/// Các hằng số định nghĩa vai trò người dùng trong hệ thống Basau Bags.
/// Khớp với ràng buộc CHECK (vaitro IN (N'Người bán', N'Khách hàng')) trong CSDL QuanLyTuiXach.
/// </summary>
public static class Roles
{
    public const string Admin = "Người bán";
    public const string Customer = "Khách hàng";
}

