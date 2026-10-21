using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DoAnLTW.Models;

namespace DoAnLTW.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbContext_TuiXach>();

        try
        {
            await db.Database.MigrateAsync();

            if (await db.TuiXachs.AnyAsync())
            {
                Console.WriteLine("[DbInitializer] CSDL đã có sẵn dữ liệu, bỏ qua bước Seed.");
                return;
            }

            Console.WriteLine("[DbInitializer] Bắt đầu nạp dữ liệu mẫu (Seeder)...");

            // nạp tài khoản (1 quản trị viên, 5 khách hàng)
            var admin = new TaiKhoan { tenDangNhap = "admin", matKhau = "123456", vaiTro = Roles.Admin };
            var tk1 = new TaiKhoan { tenDangNhap = "minhquan", matKhau = "123456", vaiTro = Roles.Customer };
            var tk2 = new TaiKhoan { tenDangNhap = "ngocanh", matKhau = "123456", vaiTro = Roles.Customer };
            var tk3 = new TaiKhoan { tenDangNhap = "thanhhoa", matKhau = "123456", vaiTro = Roles.Customer };
            var tk4 = new TaiKhoan { tenDangNhap = "quanghuy", matKhau = "123456", vaiTro = Roles.Customer };
            var tk5 = new TaiKhoan { tenDangNhap = "thimai", matKhau = "123456", vaiTro = Roles.Customer };

            db.TaiKhoans.AddRange(admin, tk1, tk2, tk3, tk4, tk5);
            await db.SaveChangesAsync();

            // nạp thông tin khách hàng liên kết với từng tài khoản
            db.KhachHangs.AddRange(
                new KhachHang { maTK = tk1.maTK, hoTen = "Nguyễn Nhật Minh Quân", email = "minhquan@gmail.com", sdt = "0901234567", diaChi = "TP. Hồ Chí Minh" },
                new KhachHang { maTK = tk2.maTK, hoTen = "Trần Ngọc Anh", email = "ngocanh@gmail.com", sdt = "0912345678", diaChi = "Bình Dương" },
                new KhachHang { maTK = tk3.maTK, hoTen = "Lê Thanh Hòa", email = "thanhhoa@gmail.com", sdt = "0923456789", diaChi = "Đồng Nai" },
                new KhachHang { maTK = tk4.maTK, hoTen = "Phạm Quang Huy", email = "quanghuy@gmail.com", sdt = "0934567890", diaChi = "Long An" },
                new KhachHang { maTK = tk5.maTK, hoTen = "Võ Thị Mai", email = "thimai@gmail.com", sdt = "0945678901", diaChi = "TP. Hồ Chí Minh" }
            );
            await db.SaveChangesAsync();

            // nạp 5 thương hiệu
            var thCK = new ThuongHieu { tenThuongHieu = "CHARLES & KEITH" };
            var thNatoli = new ThuongHieu { tenThuongHieu = "Natoli" };
            var thPedro = new ThuongHieu { tenThuongHieu = "Pedro" };
            var thElly = new ThuongHieu { tenThuongHieu = "ELLY" };
            var thJamlos = new ThuongHieu { tenThuongHieu = "Jamlos" };

            db.ThuongHieus.AddRange(thCK, thNatoli, thPedro, thElly, thJamlos);
            await db.SaveChangesAsync();

            // nạp 6 loại túi
            var ltBalo = new LoaiTui { tenLoai = "Balo" };
            var ltDeoCheo = new LoaiTui { tenLoai = "Túi đeo chéo" };
            var ltTote = new LoaiTui { tenLoai = "Túi tote" };
            var ltTiec = new LoaiTui { tenLoai = "Túi dự tiệc" };
            var ltDeoVai = new LoaiTui { tenLoai = "Túi đeo vai" };
            var ltClutch = new LoaiTui { tenLoai = "Clutch" };

            db.LoaiTuis.AddRange(ltBalo, ltDeoCheo, ltTote, ltTiec, ltDeoVai, ltClutch);
            await db.SaveChangesAsync();

            // nạp 5 chương trình khuyến mãi
            var km1 = new KhuyenMai { tenKM = "Khuyến mãi tháng 9", phanTramGiam = 10, ngayBatDau = new DateTime(2026, 9, 1), ngayKetThuc = new DateTime(2026, 9, 30), trangThai = true };
            var km2 = new KhuyenMai { tenKM = "Sale cuối tuần", phanTramGiam = 15, ngayBatDau = new DateTime(2026, 9, 5), ngayKetThuc = new DateTime(2026, 9, 30), trangThai = true };
            var km3 = new KhuyenMai { tenKM = "Ưu đãi khách hàng mới", phanTramGiam = 20, ngayBatDau = new DateTime(2026, 9, 1), ngayKetThuc = new DateTime(2026, 12, 31), trangThai = true };
            var km4 = new KhuyenMai { tenKM = "Sale mùa thu", phanTramGiam = 25, ngayBatDau = new DateTime(2026, 9, 15), ngayKetThuc = new DateTime(2026, 10, 15), trangThai = true };
            var km5 = new KhuyenMai { tenKM = "Siêu sale cuối năm", phanTramGiam = 30, ngayBatDau = new DateTime(2026, 12, 1), ngayKetThuc = new DateTime(2026, 12, 31), trangThai = true };

            db.KhuyenMais.AddRange(km1, km2, km3, km4, km5);
            await db.SaveChangesAsync();

            // nạp 15 túi xách kèm badge và mã khuyến mãi
            var t1 = new TuiXach { tenTui = "Túi đeo chéo nữ CHARLES & KEITH", maThuongHieu = thCK.maThuongHieu, maLoai = ltDeoCheo.maLoai, donGia = 1599000, soLuong = 10, moTa = "Túi đeo chéo thiết kế hiện đại, phù hợp sử dụng hằng ngày.", badge = "Sale Off", maKM = km1.maKM };
            var t2 = new TuiXach { tenTui = "Túi đeo vai nữ CHARLES & KEITH", maThuongHieu = thCK.maThuongHieu, maLoai = ltDeoVai.maLoai, donGia = 1899000, soLuong = 8, moTa = "Túi đeo vai phong cách thanh lịch, phù hợp đi làm và đi chơi.", badge = "Best Seller", maKM = km2.maKM };
            var t3 = new TuiXach { tenTui = "Clutch dự tiệc nữ CHARLES & KEITH", maThuongHieu = thCK.maThuongHieu, maLoai = ltClutch.maLoai, donGia = 1999000, soLuong = 6, moTa = "Clutch thiết kế sang trọng, phù hợp các buổi tiệc và sự kiện.", badge = "Sale Off", maKM = km3.maKM };
            var t4 = new TuiXach { tenTui = "Túi đeo chéo nữ Natoli", maThuongHieu = thNatoli.maThuongHieu, maLoai = ltDeoCheo.maLoai, donGia = 599000, soLuong = 15, moTa = "Túi đeo chéo nữ thiết kế trẻ trung, phù hợp sử dụng hằng ngày.", badge = "Best Seller", maKM = km1.maKM };
            var t5 = new TuiXach { tenTui = "Túi tote nam nữ Natoli", maThuongHieu = thNatoli.maThuongHieu, maLoai = ltTote.maLoai, donGia = 699000, soLuong = 12, moTa = "Túi tote thiết kế đơn giản, phù hợp cho cả nam và nữ.", badge = "Sale Off", maKM = km2.maKM };
            var t6 = new TuiXach { tenTui = "Balo nam Natoli", maThuongHieu = thNatoli.maThuongHieu, maLoai = ltBalo.maLoai, donGia = 799000, soLuong = 10, moTa = "Balo nam kiểu dáng năng động, phù hợp đi học và đi làm.", badge = "Sale Off", maKM = km3.maKM };
            var t7 = new TuiXach { tenTui = "Túi đeo chéo nam nữ Pedro", maThuongHieu = thPedro.maThuongHieu, maLoai = ltDeoCheo.maLoai, donGia = 799000, soLuong = 12, moTa = "Túi đeo chéo thiết kế hiện đại, phù hợp cho cả nam và nữ.", badge = "Best Seller", maKM = km1.maKM };
            var t8 = new TuiXach { tenTui = "Túi đeo vai nam Pedro", maThuongHieu = thPedro.maThuongHieu, maLoai = ltDeoVai.maLoai, donGia = 899000, soLuong = 10, moTa = "Túi đeo vai nam thiết kế thanh lịch, phù hợp đi làm và đi chơi.", badge = "Sale Off", maKM = km2.maKM };
            var t9 = new TuiXach { tenTui = "Clutch dự tiệc nữ Pedro", maThuongHieu = thPedro.maThuongHieu, maLoai = ltClutch.maLoai, donGia = 999000, soLuong = 8, moTa = "Clutch thiết kế sang trọng, phù hợp các sự kiện.", badge = "Sale Off", maKM = km4.maKM };
            var t10 = new TuiXach { tenTui = "Túi đeo chéo nữ ELLY", maThuongHieu = thElly.maThuongHieu, maLoai = ltDeoCheo.maLoai, donGia = 899000, soLuong = 10, moTa = "Túi đeo chéo nữ thiết kế sang trọng, phù hợp nhiều dịp.", badge = "Best Seller", maKM = km1.maKM };
            var t11 = new TuiXach { tenTui = "Túi đeo vai nữ ELLY", maThuongHieu = thElly.maThuongHieu, maLoai = ltDeoVai.maLoai, donGia = 1099000, soLuong = 8, moTa = "Túi đeo vai phong cách thanh lịch, phù hợp đi làm và dự tiệc.", badge = "Sale Off", maKM = km3.maKM };
            var t12 = new TuiXach { tenTui = "Balo nam nữ ELLY", maThuongHieu = thElly.maThuongHieu, maLoai = ltBalo.maLoai, donGia = 1199000, soLuong = 7, moTa = "Balo thiết kế hiện đại, phù hợp cho cả nam và nữ.", badge = "Sale Off", maKM = km4.maKM };
            var t13 = new TuiXach { tenTui = "Balo nam Jamlos", maThuongHieu = thJamlos.maThuongHieu, maLoai = ltBalo.maLoai, donGia = 699000, soLuong = 15, moTa = "Balo nam phong cách năng động, phù hợp đi học và đi làm.", badge = "New Arrival", maKM = null };
            var t14 = new TuiXach { tenTui = "Túi đeo chéo nam nữ Jamlos", maThuongHieu = thJamlos.maThuongHieu, maLoai = ltDeoCheo.maLoai, donGia = 499000, soLuong = 18, moTa = "Túi đeo chéo thiết kế nhỏ gọn, phù hợp cho cả nam và nữ.", badge = "Sale Off", maKM = km2.maKM };
            var t15 = new TuiXach { tenTui = "Túi tote nam nữ Jamlos", maThuongHieu = thJamlos.maThuongHieu, maLoai = ltTote.maLoai, donGia = 599000, soLuong = 12, moTa = "Túi tote thiết kế đơn giản, tiện dụng và năng động.", badge = "Best Seller", maKM = null };

            var allTuis = new[] { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15 };
            db.TuiXachs.AddRange(allTuis);
            await db.SaveChangesAsync();

            // nạp 45 hình ảnh túi (3 góc ảnh / túi)
            var hinhAnhs = new List<HinhAnhTui>();
            for (int i = 0; i < allTuis.Length; i++)
            {
                int tuiNum = i + 1;
                int maTuiId = allTuis[i].maTui;
                hinhAnhs.Add(new HinhAnhTui { maTui = maTuiId, tenHinhAnh = $"tui{tuiNum}_1.png", laAnhChinh = true, thuTu = 1 });
                hinhAnhs.Add(new HinhAnhTui { maTui = maTuiId, tenHinhAnh = $"tui{tuiNum}_2.png", laAnhChinh = false, thuTu = 2 });
                hinhAnhs.Add(new HinhAnhTui { maTui = maTuiId, tenHinhAnh = $"tui{tuiNum}_3.png", laAnhChinh = false, thuTu = 3 });
            }
            db.HinhAnhTuis.AddRange(hinhAnhs);
            await db.SaveChangesAsync();

            Console.WriteLine("[DbInitializer] Nạp dữ liệu mẫu Seeder thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Lỗi khi nạp dữ liệu: {ex.Message}");
        }
    }
}
