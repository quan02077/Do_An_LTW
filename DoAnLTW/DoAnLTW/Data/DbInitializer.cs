using DoAnLTW.Models;
using DoAnLTW.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Data;

/// <summary>
/// Dữ liệu chạy thử cho môi trường phát triển. Chỉ thêm khi database chưa có tài khoản.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbContext_TuiXach>();
        await db.Database.MigrateAsync();

        if (await db.TaiKhoans.AnyAsync())
        {
            return;
        }

        var accountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var customerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var brandId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var categoryId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var promotionId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var toteBagId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var shoulderBagId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var cartId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        var orderId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        var account = new TaiKhoan
        {
            maTK = accountId,
            tenDangNhap = "customer.demo",
            vaiTro = "KhachHang"
        };
        account.matKhau = new PasswordHasher<TaiKhoan>().HashPassword(account, "Customer@123");

        var promotion = new KhuyenMai
        {
            maKM = promotionId,
            tenKM = "Ưu đãi khai trương",
            phanTramGiam = 10,
            ngayBatDau = DateTime.Now.AddDays(-30),
            ngayKetThuc = DateTime.Now.AddDays(30),
            trangThai = true
        };
        var toteBag = new TuiXach
        {
            maTui = toteBagId,
            tenTui = "Túi tote Classic",
            maThuongHieu = brandId,
            maLoai = categoryId,
            maKM = promotionId,
            donGia = 550000,
            soLuong = 22,
            moTa = "Sản phẩm mẫu để kiểm thử giỏ hàng và đơn hàng."
        };
        var shoulderBag = new TuiXach
        {
            maTui = shoulderBagId,
            tenTui = "Túi đeo vai Everyday",
            maThuongHieu = brandId,
            maLoai = categoryId,
            donGia = 780000,
            soLuong = 15,
            moTa = "Sản phẩm mẫu để kiểm thử giỏ hàng và đơn hàng."
        };
        var order = new DonHang
        {
            maDH = orderId,
            maKH = customerId,
            ngayDat = DateTime.Now.AddDays(-2),
            trangThai = OrderStatus.HoanTat,
            diaChiNhan = "12 Nguyễn Văn Bảo, Phường 4, Gò Vấp, TP. Hồ Chí Minh",
            tongTien = 990000
        };
        order.ChiTietDonHangs.Add(new ChiTietDonHang
        {
            maDH = orderId,
            maTui = toteBagId,
            soLuong = 2,
            donGia = 495000,
            thanhTien = 990000
        });

        db.AddRange(
            account,
            new KhachHang
            {
                maKH = customerId,
                matk = accountId,
                hoTen = "Khách hàng Demo",
                email = "customer.demo@example.com",
                sdt = "0900000000",
                diaChi = "TP. Hồ Chí Minh"
            },
            new ThuongHieu { maThuongHieu = brandId, tenThuongHieu = "Luna Bags" },
            new LoaiTui { maLoai = categoryId, tenLoai = "Túi xách nữ" },
            promotion,
            toteBag,
            shoulderBag,
            new GioHang
            {
                maGioHang = cartId,
                maKH = customerId,
                ngayTao = DateTime.Now,
                ChiTietGioHangs = new List<ChiTietGioHang>
                {
                    new() { maGioHang = cartId, maTui = shoulderBagId, soLuong = 1 }
                }
            },
            order);

        await db.SaveChangesAsync();
    }
}
