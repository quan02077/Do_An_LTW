using DoAnLTW.Models;
using Microsoft.AspNetCore.Identity;

namespace DoAnLTW.Services;

public interface IPasswordService
{
    string Hash(TaiKhoan account, string password);
    bool Verify(TaiKhoan account, string password);

    // true nếu chuỗi đã là hash của PasswordHasher (dùng để chuyển dữ liệu plaintext cũ).
    bool IsHashed(string? storedValue);
}

/// <summary>
/// bọc PasswordHasher của ASP.NET Core Identity: hash PBKDF2 + salt ngẫu nhiên,
/// mỗi lần hash cùng một mật khẩu cho ra chuỗi khác nhau nên không thể so sánh bằng ==.
/// PasswordHasher không giữ trạng thái nên đăng ký Singleton là an toàn.
/// </summary>
public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<TaiKhoan> hasher = new();

    public string Hash(TaiKhoan account, string password) => hasher.HashPassword(account, password);

    public bool Verify(TaiKhoan account, string password)
    {
        if (!IsHashed(account.matKhau)) return false; // không còn chấp nhận mật khẩu plaintext

        return hasher.VerifyHashedPassword(account, account.matKhau, password) != PasswordVerificationResult.Failed;
    }

    public bool IsHashed(string? storedValue)
    {
        // hash định dạng v3 là Base64 của mảng byte bắt đầu bằng 0x01 (chuỗi bắt đầu "AQAAAA").
        if (string.IsNullOrEmpty(storedValue) || !storedValue.StartsWith("AQAAAA", StringComparison.Ordinal))
        {
            return false;
        }

        var buffer = new byte[storedValue.Length];
        return Convert.TryFromBase64String(storedValue, buffer, out var length) && length > 13 && buffer[0] == 0x01;
    }
}
