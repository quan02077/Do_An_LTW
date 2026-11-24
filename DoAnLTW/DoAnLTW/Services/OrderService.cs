using System.Data;
using DoAnLTW.Data;
using DoAnLTW.Models;
using DoAnLTW.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Services;

public static class OrderStatus
{
    public const string ChoDuyet = "Chờ duyệt";
    public const string DangGiao = "Đang giao";
    public const string HoanTat = "Hoàn tất";
    public const string DaHuy = "Đã hủy";

    public static readonly IReadOnlyList<string> All = [ChoDuyet, DangGiao, HoanTat, DaHuy];

    // luồng trạng thái hợp lệ: Hoàn tất và Đã hủy là trạng thái kết thúc, không đổi được nữa.
    private static readonly Dictionary<string, string[]> Transitions = new()
    {
        [ChoDuyet] = [DangGiao, DaHuy],
        [DangGiao] = [HoanTat, DaHuy],
        [HoanTat] = [],
        [DaHuy] = []
    };

    public static IReadOnlyList<string> GetNextStatuses(string current) =>
        Transitions.TryGetValue(current, out var next) ? next : [];

    public static bool CanTransition(string from, string to) => GetNextStatuses(from).Contains(to);
}

// lưu mã không dấu trong CSDL, hiển thị nhãn tiếng Việt trên giao diện
public static class PaymentMethod
{
    public const string COD = "COD";
    public const string ChuyenKhoan = "ChuyenKhoan";

    public static readonly IReadOnlyDictionary<string, string> Labels = new Dictionary<string, string>
    {
        [COD] = "Thanh toán khi nhận hàng (COD)",
        [ChuyenKhoan] = "Chuyển khoản ngân hàng"
    };

    public static bool IsValid(string? code) => code is not null && Labels.ContainsKey(code);
    public static string GetLabel(string? code) => code is not null && Labels.TryGetValue(code, out var label) ? label : "Không rõ";
}

public interface IOrderService
{
    Task<OrderViewModel> CheckoutAsync(int maKh, CheckoutViewModel request, CancellationToken cancellationToken = default);
    Task<CheckoutViewModel> GetCheckoutDefaultsAsync(int maKh, IEnumerable<int> selectedMaTui, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrderViewModel>> GetHistoryAsync(int maKh, CancellationToken cancellationToken = default);
    Task CancelAsync(int maKh, int maDh, CancellationToken cancellationToken = default);
}

public class OrderService(DbContext_TuiXach db, IProductPricingService pricingService, INotificationService notificationService, IEmailService emailService) : IOrderService
{
    public async Task<OrderViewModel> CheckoutAsync(int maKh, CheckoutViewModel request, CancellationToken ct = default)
    {
        DonHang order;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var cart = await db.GioHangs
                .Include(x => x.ChiTietGioHangs)
                    .ThenInclude(x => x.TuiXach)
                        .ThenInclude(x => x!.KhuyenMai)
                .Include(x => x.ChiTietGioHangs)
                    .ThenInclude(x => x.TuiXach)
                        .ThenInclude(x => x!.HinhAnhTuis)
                .SingleOrDefaultAsync(x => x.maKH == maKh, ct);

            if (cart is null || cart.ChiTietGioHangs.Count == 0)
            {
                throw new InvalidOperationException("Giỏ hàng đang trống.");
            }

            // không tin danh sách mã túi từ form: chỉ lấy những dòng thật sự có trong giỏ của khách này.
            var selectedIds = request.SelectedMaTui.Distinct().ToList();
            var selectedItems = cart.ChiTietGioHangs.Where(x => selectedIds.Contains(x.maTui)).ToList();
            if (selectedItems.Count == 0)
            {
                throw new InvalidOperationException("Vui lòng chọn ít nhất một sản phẩm để thanh toán.");
            }
            if (selectedItems.Count != selectedIds.Count)
            {
                throw new InvalidOperationException("Giỏ hàng đã thay đổi, vui lòng kiểm tra lại các sản phẩm đã chọn.");
            }

            if (!PaymentMethod.IsValid(request.PhuongThucThanhToan))
            {
                throw new InvalidOperationException("Phương thức thanh toán không hợp lệ.");
            }

            foreach (var item in selectedItems)
            {
                if (item.TuiXach is null || item.soLuong > item.TuiXach.soLuong)
                {
                    throw new InvalidOperationException("Một hoặc nhiều sản phẩm không đủ số lượng tồn kho.");
                }
            }

            order = new DonHang
            {
                maKH = maKh,
                ngayDat = DateTime.Now,
                trangThai = OrderStatus.ChoDuyet,
                diaChiNhan = request.DiaChiNhan.Trim(),
                tenNguoiNhan = request.TenNguoiNhan.Trim(),
                sdtNguoiNhan = request.SdtNguoiNhan.Trim(),
                phuongThucThanhToan = request.PhuongThucThanhToan,
                ghiChu = string.IsNullOrWhiteSpace(request.GhiChu) ? null : request.GhiChu.Trim()
            };

            foreach (var cartItem in selectedItems)
            {
                var product = cartItem.TuiXach!;
                var price = pricingService.GetPrice(product, order.ngayDat);
                order.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    maTui = product.maTui,
                    soLuong = cartItem.soLuong,
                    donGia = price.DonGiaApDung,
                    thanhTien = price.DonGiaApDung * cartItem.soLuong
                });
                product.soLuong -= cartItem.soLuong;
            }

            order.tongTien = order.ChiTietDonHangs.Sum(x => x.thanhTien);
            db.DonHangs.Add(order);
            // chỉ xóa khỏi giỏ những sản phẩm đã đặt; sản phẩm không chọn vẫn nằm lại trong giỏ
            db.ChiTietGioHangs.RemoveRange(selectedItems);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        // gửi thông báo sau khi đã commit giao dịch đặt hàng
        await notificationService.NotifyOrderCreatedAsync(ToNotification(order), ct);

        // 👉 THÊM ĐOẠN GỬI EMAIL NÀY:
        try
        {
            var khachHang = await db.KhachHangs.FindAsync([maKh], ct);
            if (khachHang != null && !string.IsNullOrWhiteSpace(khachHang.email))
            {
                await emailService.SendOrdersConfirmationAsync(khachHang.email, khachHang.hoTen, Map(order), ct);
            }
        }
        catch (Exception ex)
        {
            // Đặt trong try/catch để lỡ mất mạng Gmail thì đơn hàng vẫn được lưu thành công
            Console.WriteLine($"[Email Error] Không gửi được mail: {ex.Message}");
        }

        return Map(order);
    }

    // điền sẵn người nhận từ hồ sơ khách hàng; khách vẫn sửa được nếu giao cho người khác
    public async Task<CheckoutViewModel> GetCheckoutDefaultsAsync(int maKh, IEnumerable<int> selectedMaTui, CancellationToken ct = default)
    {
        var customer = await db.KhachHangs.AsNoTracking().SingleOrDefaultAsync(x => x.maKH == maKh, ct);
        return new CheckoutViewModel
        {
            SelectedMaTui = selectedMaTui.Distinct().ToList(),
            TenNguoiNhan = customer?.hoTen ?? string.Empty,
            SdtNguoiNhan = customer?.sdt ?? string.Empty,
            DiaChiNhan = customer?.diaChi ?? string.Empty,
            PhuongThucThanhToan = PaymentMethod.COD
        };
    }

    public async Task<IReadOnlyCollection<OrderViewModel>> GetHistoryAsync(int maKh, CancellationToken ct = default)
    {
        var orders = await db.DonHangs
            .Where(x => x.maKH == maKh)
            .Include(x => x.ChiTietDonHangs).ThenInclude(x => x.TuiXach).ThenInclude(x => x!.HinhAnhTuis)
            .OrderByDescending(x => x.ngayDat)
            .ToListAsync(ct);

        return orders.Select(Map).ToList();
    }

    public async Task CancelAsync(int maKh, int maDh, CancellationToken ct = default)
    {
        DonHang order;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            order = await db.DonHangs
                .Include(x => x.ChiTietDonHangs).ThenInclude(x => x.TuiXach)
                .SingleOrDefaultAsync(x => x.maDH == maDh && x.maKH == maKh, ct)
                ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

            if (order.trangThai != OrderStatus.ChoDuyet)
            {
                throw new InvalidOperationException("Chỉ có thể hủy đơn đang chờ duyệt.");
            }

            order.trangThai = OrderStatus.DaHuy;
            foreach (var detail in order.ChiTietDonHangs)
            {
                if (detail.TuiXach is not null)
                {
                    detail.TuiXach.soLuong += detail.soLuong;
                }
            }

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        // gửi thông báo sau khi đã commit giao dịch hủy đơn
        await notificationService.NotifyOrderCancelledByCustomerAsync(ToNotification(order), ct);
    }

    private static OrderNotification ToNotification(DonHang order) => new(
        order.maDH,
        order.MaHienThi,
        order.maKH,
        order.tenNguoiNhan,
        order.tongTien,
        order.ngayDat,
        order.trangThai);

    private static OrderViewModel Map(DonHang order) => new(
        order.maDH,
        order.ngayDat,
        order.trangThai,
        order.diaChiNhan,
        order.tongTien,
        order.ChiTietDonHangs.Select(x => new OrderItemViewModel(
            x.maTui,
            x.TuiXach?.tenTui ?? "Sản phẩm đã ngừng bán",
            x.TuiXach?.HinhAnhTuis.OrderBy(a => a.thuTu).FirstOrDefault()?.tenHinhAnh,
            x.soLuong,
            x.donGia,
            x.thanhTien)).ToList(),
        order.tenNguoiNhan,
        order.sdtNguoiNhan,
        order.phuongThucThanhToan,
        order.ghiChu);
}
