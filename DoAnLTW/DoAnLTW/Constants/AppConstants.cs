namespace DoAnLTW.Constants;

/// <summary>
/// Quản lý toàn bộ hằng số (constants) của hệ thống:
/// Loại bỏ hoàn toàn magic-strings, dễ dàng bảo trì và refactor.
/// </summary>
public static class AppConstants
{
    /// <summary>Các khóa lưu trữ thông báo ngắn (TempData)</summary>
    public static class TempDataKeys
    {
        public const string CartSuccess = "CartSuccess";
        public const string CartError = "CartError";
        public const string AuthSuccess = "AuthSuccess";
        public const string AuthError = "AuthError";
        public const string OrderSuccess = "OrderSuccess";
        public const string OrderError = "OrderError";
        public const string ProfileSuccess = "ProfileSuccess";
        public const string PasswordError = "PasswordError";
    }

    /// <summary>Trạng thái vòng đời của đơn hàng</summary>
    public static class OrderStatuses
    {
        public const string ChoDuyet = "Chờ duyệt";
        public const string DangGiao = "Đang giao";
        public const string HoanTat = "Hoàn tất";
        public const string DaHuy = "Đã hủy";

        public static readonly string[] All = [ChoDuyet, DangGiao, HoanTat, DaHuy];
    }

    /// <summary>Phương thức thanh toán được hỗ trợ</summary>
    public static class PaymentMethods
    {
        public const string COD = "COD";
        public const string ChuyenKhoan = "ChuyenKhoan";

        public const string LabelCOD = "Thanh toán khi nhận hàng (COD)";
        public const string LabelChuyenKhoan = "Chuyển khoản ngân hàng";
    }

    /// <summary>Vai trò người dùng trong hệ thống</summary>
    public static class Roles
    {
        public const string Admin = "Người bán";
        public const string Customer = "Khách hàng";
    }

    /// <summary>Tiền tố khóa bộ nhớ tạm (Cache keys)</summary>
    public static class CacheKeys
    {
        public const string PasswordResetPrefix = "pwd_reset_";
    }

    /// <summary>Hằng số cho dịch vụ gửi Email</summary>
    public static class Email
    {
        public const int DefaultSmtpPort = 587;
        public const int SmtpTimeoutMs = 15_000;
        public const int QueueCapacity = 100;
        public const int JobTimeoutSeconds = 30;
    }

    /// <summary>Tên các nhóm (groups) và kênh SignalR</summary>
    public static class ChatHub
    {
        public const string StaffGroup = "support-staff";
        public const string CustomersGroup = "support-customers";
        public const string CustomerGroupPrefix = "support-customer-";
        public static string CustomerGroup(int maKh) => $"{CustomerGroupPrefix}{maKh}";
    }

    /// <summary>Tên các chính sách giới hạn tốc độ (Rate Limiting)</summary>
    public static class RateLimitPolicies
    {
        public const string Chat = "chat";
    }
}
