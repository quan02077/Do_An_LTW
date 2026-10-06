using DoAnLTW.Data;
using DoAnLTW.Models;
using DoAnLTW.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Services;

public interface ICartService
{
    Task<CartViewModel> GetAsync(Guid maKh, CancellationToken cancellationToken = default);
    Task<CartViewModel> AddAsync(Guid maKh, AddCartItemViewModel request, CancellationToken cancellationToken = default);
    Task<CartViewModel> UpdateAsync(Guid maKh, Guid maTui, UpdateCartItemViewModel request, CancellationToken cancellationToken = default);
    Task<CartViewModel> RemoveAsync(Guid maKh, Guid maTui, CancellationToken cancellationToken = default);
}

public class CartService(DbContext_TuiXach db, IProductPricingService pricingService) : ICartService
{
    public async Task<CartViewModel> GetAsync(Guid maKh, CancellationToken ct = default)
    {
        var cart = await GetOrCreateAsync(maKh, ct);
        await db.Entry(cart).Collection(x => x.ChiTietGioHangs).Query()
            .Include(x => x.TuiXach).ThenInclude(x => x!.HinhAnhTuis)
            .Include(x => x.TuiXach).ThenInclude(x => x!.KhuyenMai)
            .LoadAsync(ct);

        var items = cart.ChiTietGioHangs.Select(x =>
        {
            var gia = pricingService.GetPrice(x.TuiXach!);
            return new CartItemViewModel(
                x.maTui,
                x.TuiXach!.tenTui,
                x.soLuong,
                gia.DonGiaApDung,
                gia.DonGiaApDung * x.soLuong,
                x.TuiXach.HinhAnhTuis.OrderBy(a => a.thuTu).FirstOrDefault()?.tenHinhAnh);
        }).ToList();
        return new CartViewModel(items, items.Sum(x => x.SoLuong), items.Sum(x => x.ThanhTien));
    }

    public async Task<CartViewModel> AddAsync(Guid maKh, AddCartItemViewModel request, CancellationToken ct = default)
    {
        var product = await db.TuiXachs.FindAsync([request.MaTui], ct) ?? throw new KeyNotFoundException("Không tìm thấy túi xách.");
        var cart = await GetOrCreateAsync(maKh, ct);
        var item = await db.ChiTietGioHangs.FindAsync([cart.maGioHang, request.MaTui], ct);
        var quantity = (item?.soLuong ?? 0) + request.SoLuong;
        if (quantity > product.soLuong) throw new InvalidOperationException("Số lượng vượt quá tồn kho.");
        if (item is null) db.ChiTietGioHangs.Add(new ChiTietGioHang { maGioHang = cart.maGioHang, maTui = request.MaTui, soLuong = request.SoLuong }); else item.soLuong = quantity;
        await db.SaveChangesAsync(ct);
        return await GetAsync(maKh, ct);
    }

    public async Task<CartViewModel> UpdateAsync(Guid maKh, Guid maTui, UpdateCartItemViewModel request, CancellationToken ct = default)
    {
        var cart = await GetOrCreateAsync(maKh, ct);
        var item = await db.ChiTietGioHangs.FindAsync([cart.maGioHang, maTui], ct) ?? throw new KeyNotFoundException("Sản phẩm không có trong giỏ.");
        var product = await db.TuiXachs.FindAsync([maTui], ct) ?? throw new KeyNotFoundException("Không tìm thấy túi xách.");
        if (request.SoLuong > product.soLuong) throw new InvalidOperationException("Số lượng vượt quá tồn kho.");
        item.soLuong = request.SoLuong; await db.SaveChangesAsync(ct); return await GetAsync(maKh, ct);
    }

    public async Task<CartViewModel> RemoveAsync(Guid maKh, Guid maTui, CancellationToken ct = default)
    {
        var cart = await GetOrCreateAsync(maKh, ct); var item = await db.ChiTietGioHangs.FindAsync([cart.maGioHang, maTui], ct);
        if (item is not null) { db.ChiTietGioHangs.Remove(item); await db.SaveChangesAsync(ct); }
        return await GetAsync(maKh, ct);
    }

    private async Task<GioHang> GetOrCreateAsync(Guid maKh, CancellationToken ct)
    {
        var cart = await db.GioHangs.SingleOrDefaultAsync(x => x.maKH == maKh, ct);
        if (cart is not null) return cart;
        cart = new GioHang { maGioHang = Guid.NewGuid(), maKH = maKh, ngayTao = DateTime.Now }; db.GioHangs.Add(cart); await db.SaveChangesAsync(ct); return cart;
    }
}
