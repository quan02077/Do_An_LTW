namespace DoAnLTW.Services;

/// <summary>
/// kết quả trả về từ service cho các lỗi nghiệp vụ "dự kiến được" (trùng email, không được xóa...).
/// controller đọc Error để hiển thị lên form/TempData thay vì để exception thành lỗi 500.
/// Field: tên property trên ViewModel để gắn lỗi vào đúng ô nhập (null = lỗi chung).
/// </summary>
public record ServiceResult(bool Success, string? Error = null, string? Field = null)
{
    public static ServiceResult Ok() => new(true);
    public static ServiceResult Fail(string error, string? field = null) => new(false, error, field);
}

public record ServiceResult<T>(bool Success, T? Value, string? Error = null, string? Field = null)
{
    public static ServiceResult<T> Ok(T value) => new(true, value);
    public static ServiceResult<T> Fail(string error, string? field = null) => new(false, default, error, field);
}
