using System.Net;
using DoAnLTW.ViewModels;
using MailKit;   // cần cho các hàm mở rộng SendAsync(MimeMessage, CancellationToken)
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DoAnLTW.Services
{
    public interface IEmailService
    {
        Task SendOrdersConfirmationAsync(string to, string name, OrderViewModel order, CancellationToken ct = default);
        Task SendPasswordResetAsync(string to, string name, string resetLink, CancellationToken ct = default);
    }

    /// <summary>
    /// gửi email xác nhận đơn hàng qua SMTP. Cấu hình ở mục "EmailSettings": SenderEmail và AppPassword KHÔNG đặt trong appsettings.json
    /// (repo công khai) mà đặt bằng .env (EmailSettings__SenderEmail, EmailSettings__AppPassword), user-secrets hoặc biến môi trường.
    /// Thiếu SenderEmail/AppPassword thì không gửi gì (bỏ qua êm).
    /// </summary>
    public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
    {
        // giới hạn thời gian mỗi lần gửi để không treo luồng nền khi mạng chậm
        private const int SmtpTimeoutMs = 15_000;

        public async Task SendOrdersConfirmationAsync(string to, string name, OrderViewModel order, CancellationToken ct = default)
        {
            var smtpServer = config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var port = int.Parse(config["EmailSettings:Port"] ?? "587");
            var senderName = config["EmailSettings:SenderName"] ?? "Shop Túi Xách BASAU";
            var senderEmail = config["EmailSettings:SenderEmail"] ?? "";
            var appPassword = config["EmailSettings:AppPassword"] ?? "";
            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword))
            {
                logger.LogDebug("Chưa cấu hình EmailSettings:SenderEmail/AppPassword, bỏ qua gửi mail xác nhận đơn.");
                return;
            }

            var smtpServer = config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var port = int.TryParse(config["EmailSettings:Port"], out var p) ? p : 587;
            var senderName = config["EmailSettings:SenderName"] ?? "Shop Túi Xách BASAU";
            var secure = Enum.TryParse<SecureSocketOptions>(config["EmailSettings:SecureSocket"], ignoreCase: true, out var s) ? s : SecureSocketOptions.StartTls;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(name, to));
            message.Subject = $"[Xác nhận đơn hàng #{order.MaHienThi}] - Cảm ơn bạn đã mua sắm!";
            message.Body = new BodyBuilder { HtmlBody = BuildHtmlBody(name, senderName, order) }.ToMessageBody();

            using var client = new SmtpClient { Timeout = SmtpTimeoutMs };
            await client.ConnectAsync(smtpServer, port, secure, ct);
            await client.AuthenticateAsync(senderEmail, appPassword, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }

        // MỌI chuỗi do khách nhập (tên, địa chỉ, số điện thoại, tên sản phẩm...) phải mã hóa HTML trước khi chèn vào thư:
        // nếu không, kẻ xấu đặt tên người nhận chứa liên kết giả để shop gửi thư lừa đảo từ địa chỉ gửi chính thức.
        internal static string BuildHtmlBody(string name, string senderName, OrderViewModel order)
        {
            static string E(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);

            var rows = string.Concat(order.Items.Select(item => $@"
                <tr>
                    <td style='padding: 8px; border-bottom: 1px solid #eee;'>{E(item.TenTui)}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #eee; text-align: center;'>{item.SoLuong}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #eee; text-align: right;'>{item.ThanhTien:N0} đ</td>
                </tr>"));

            return $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px;'>
            <h2 style='color: #111827; text-align: center; border-bottom: 2px solid #111827; padding-bottom: 10px;'>XÁC NHẬN ĐƠN HÀNG THÀNH CÔNG</h2>
            <p>Xin chào <strong>{E(name)}</strong>,</p>
            <p>Cảm ơn bạn đã đặt hàng tại <strong>{E(senderName)}</strong>. Đơn hàng của bạn đã được tiếp nhận và đang được xử lý.</p>

            <div style='background-color: #f9fafb; padding: 12px; border-radius: 6px; margin: 15px 0;'>
                <p style='margin: 4px 0;'><strong>Mã đơn hàng:</strong> #{E(order.MaHienThi)}</p>
                <p style='margin: 4px 0;'><strong>Ngày đặt:</strong> {order.NgayDat:dd/MM/yyyy HH:mm}</p>
                <p style='margin: 4px 0;'><strong>Người nhận:</strong> {E(order.TenNguoiNhan)} - {E(order.SdtNguoiNhan)}</p>
                <p style='margin: 4px 0;'><strong>Địa chỉ giao:</strong> {E(order.DiaChiNhan)}</p>
            </div>
            <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                <thead>
                    <tr style='background-color: #111827; color: white;'>
                        <th style='padding: 8px; text-align: left;'>Sản phẩm</th>
                        <th style='padding: 8px; text-align: center;'>SL</th>
                        <th style='padding: 8px; text-align: right;'>Thành tiền</th>
                    </tr>
                </thead>
                <tbody>{rows}
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan='2' style='padding: 12px 8px; font-weight: bold; text-align: right;'>Tổng thanh toán:</td>
                        <td style='padding: 12px 8px; font-weight: bold; text-align: right; color: #dc2626; font-size: 16px;'>{order.TongTien:N0} đ</td>
                    </tr>
                </tfoot>
            </table>
            <p style='margin-top: 25px; font-size: 13px; color: #6b7280; text-align: center;'>
                Mọi thắc mắc xin vui lòng liên hệ hotline hoặc phản hồi trực tiếp email này.<br/>
                Chúc bạn một ngày tuyệt vời!
            </p>
        </div>";
        }

        public async Task SendPasswordResetAsync(string to, string name, string resetLink, CancellationToken ct = default)
        {
            var smtpServer = config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var port = int.Parse(config["EmailSettings:Port"] ?? "587");
            var senderName = config["EmailSettings:SenderName"] ?? "Shop Túi Xách BASAU";
            var senderEmail = config["EmailSettings:SenderEmail"] ?? "";
            var appPassword = config["EmailSettings:AppPassword"] ?? "";

            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword)) return;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(name, to));
            message.Subject = $"[Yêu cầu đặt lại mật khẩu] - {senderName}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
        <div style='font-family: Arial, sans-serif; max-width: 550px; margin: 0 auto; border: 1px solid #e5e7eb; border-radius: 8px; padding: 25px;'>
            <h2 style='color: #111827; text-align: center; border-bottom: 2px solid #111827; padding-bottom: 12px;'>ĐẶT LẠI MẬT KHẨU</h2>
            <p>Xin chào <strong>{name}</strong>,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản liên kết với email <strong>{to}</strong> tại {senderName}.</p>
            <p>Vui lòng bấm vào nút bên dưới để tiến hành đổi mật khẩu mới:</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{resetLink}' style='background-color: #111827; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block; letter-spacing: 0.5px;'>
                    ĐẶT LẠI MẬT KHẨU NGAY
                </a>
            </div>

            <p style='color: #ef4444; font-size: 13px;'>* <strong>Lưu ý:</strong> Liên kết này chỉ có hiệu lực trong vòng <strong>15 phút</strong> vì lý do bảo mật.</p>
            <p style='color: #6b7280; font-size: 13px;'>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email. Mật khẩu của bạn vẫn được an toàn.</p>
            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
            <p style='font-size: 12px; color: #9ca3af; text-align: center;'>{senderName} - Hệ thống bán lẻ túi xách thời trang cao cấp</p>
        </div>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(senderEmail, appPassword, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
