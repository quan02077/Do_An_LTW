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
}

public interface IOrderService
{
    Task<OrderViewModel> CheckoutAsync(Guid maKh, CheckoutViewModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrderViewModel>> GetHistoryAsync(Guid maKh, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid maKh, Guid maDh, CancellationToken cancellationToken = default);
}

public class OrderService(DbContext_TuiXach db, IProductPricingService pricingService) : IOrderService
{
    public async Task<OrderViewModel> CheckoutAsync(Guid maKh, CheckoutViewModel request, CancellationToken ct = default)
    {
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

            foreach (var item in cart.ChiTietGioHangs)
            {
                if (item.TuiXach is null || item.soLuong > item.TuiXach.soLuong)
                {
                    throw new InvalidOperationException("Một hoặc nhiều sản phẩm không đủ số lượng tồn kho.");
                }
            }

            var order = new DonHang
            {
                maDH = Guid.NewGuid(),
                maKH = maKh,
                ngayDat = DateTime.Now,
                trangThai = OrderStatus.ChoDuyet,
                diaChiNhan = request.DiaChiNhan.Trim()
            };

            foreach (var cartItem in cart.ChiTietGioHangs)
            {
                var product = cartItem.TuiXach!;
                var price = pricingService.GetPrice(product, order.ngayDat);
                order.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    maDH = order.maDH,
                    maTui = product.maTui,
                    soLuong = cartItem.soLuong,
                    // Snapshot: giá chốt được lưu trực tiếp, không phụ thuộc giá hiện tại của sản phẩm.
                    donGia = price.DonGiaApDung,
                    thanhTien = price.DonGiaApDung * cartItem.soLuong
                });
                product.soLuong -= cartItem.soLuong;
            }

            order.tongTien = order.ChiTietDonHangs.Sum(x => x.thanhTien);
            db.DonHangs.Add(order);
            db.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Map(order);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<OrderViewModel>> GetHistoryAsync(Guid maKh, CancellationToken ct = default)
    {
        var orders = await db.DonHangs
            .Where(x => x.maKH == maKh)
            .Include(x => x.ChiTietDonHangs).ThenInclude(x => x.TuiXach).ThenInclude(x => x!.HinhAnhTuis)
            .OrderByDescending(x => x.ngayDat)
            .ToListAsync(ct);

        return orders.Select(Map).ToList();
    }

    public async Task CancelAsync(Guid maKh, Guid maDh, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var order = await db.DonHangs
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
    }

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
            x.thanhTien)).ToList());
}
