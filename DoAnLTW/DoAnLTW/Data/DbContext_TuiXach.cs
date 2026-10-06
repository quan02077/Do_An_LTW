using Microsoft.EntityFrameworkCore;
using DoAnLTW.Models;

namespace DoAnLTW.Data
{
    public class DbContext_TuiXach : DbContext
    {
        public DbContext_TuiXach(DbContextOptions<DbContext_TuiXach> options) : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<ThuongHieu> ThuongHieus { get; set; }
        public DbSet<LoaiTui> LoaiTuis { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<TuiXach> TuiXachs { get; set; }
        public DbSet<HinhAnhTui> HinhAnhTuis { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Tài khoản — Khách hàng: 1-1 (Cascade)
            modelBuilder.Entity<KhachHang>()
                .HasOne(kh => kh.TaiKhoan)
                .WithOne(tk => tk.KhachHang)
                .HasForeignKey<KhachHang>(kh => kh.matk)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Khách hàng — Giỏ hàng: 1-1 (Cascade)
            modelBuilder.Entity<GioHang>()
                .HasOne(gh => gh.KhachHang)
                .WithOne(kh => kh.GioHang)
                .HasForeignKey<GioHang>(gh => gh.maKH)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Giỏ hàng — Chi tiết giỏ hàng: 1-N (Cascade)
            modelBuilder.Entity<ChiTietGioHang>()
                .HasKey(ct => new { ct.maGioHang, ct.maTui });

            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(ct => ct.GioHang)
                .WithMany(gh => gh.ChiTietGioHangs)
                .HasForeignKey(ct => ct.maGioHang)
                .OnDelete(DeleteBehavior.Cascade);

            // Túi xách — Chi tiết giỏ hàng: 1-N (Cascade)
            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(ct => ct.TuiXach)
                .WithMany(tx => tx.ChiTietGioHangs)
                .HasForeignKey(ct => ct.maTui)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietGioHang>()
                .ToTable(t => t.HasCheckConstraint("CK_ChiTietGioHang_SoLuong", "[soLuong] > 0"));

            // 4. Túi xách — Hình ảnh: 1-N (Cascade)
            modelBuilder.Entity<HinhAnhTui>()
                .HasOne(ha => ha.TuiXach)
                .WithMany(tx => tx.HinhAnhTuis)
                .HasForeignKey(ha => ha.maTui)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. Khách hàng — Đơn hàng: 1-N (Restrict - không cho xóa khách khi có lịch sử đơn hàng)
            modelBuilder.Entity<DonHang>()
                .HasOne(dh => dh.KhachHang)
                .WithMany(kh => kh.DonHangs)
                .HasForeignKey(dh => dh.maKH)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Đơn hàng — Chi tiết đơn hàng: 1-N (Cascade)
            modelBuilder.Entity<ChiTietDonHang>()
                .HasKey(ct => new { ct.maDH, ct.maTui });

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DonHang)
                .WithMany(dh => dh.ChiTietDonHangs)
                .HasForeignKey(ct => ct.maDH)
                .OnDelete(DeleteBehavior.Cascade);

            // Túi xách — Chi tiết đơn hàng: 1-N (Restrict - không cho xóa túi khi đã có trong lịch sử đơn hàng)
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.TuiXach)
                .WithMany(tx => tx.ChiTietDonHangs)
                .HasForeignKey(ct => ct.maTui)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChiTietDonHang>()
                .ToTable(t => t.HasCheckConstraint("CK_ChiTietDonHang_SoLuong", "[soLuong] > 0"));

            // Những trạng thái này được dùng nhất quán ở luồng khách hàng và trang quản trị.
            modelBuilder.Entity<DonHang>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint("CK_DonHang_TongTien", "[tongTien] >= 0");
                    t.HasCheckConstraint("CK_DonHang_TrangThai", "[trangThai] IN (N'Chờ duyệt', N'Đang giao', N'Hoàn tất', N'Đã hủy')");
                });

            modelBuilder.Entity<TuiXach>()
                .ToTable(t => t.HasCheckConstraint("CK_TuiXach_SoLuong", "[soLuong] >= 0"));

            // 7. Thương hiệu — Túi xách: 1-N (Restrict - không cho xóa thương hiệu nếu còn túi)
            modelBuilder.Entity<TuiXach>()
                .HasOne(tx => tx.ThuongHieu)
                .WithMany(th => th.TuiXachs)
                .HasForeignKey(tx => tx.maThuongHieu)
                .OnDelete(DeleteBehavior.Restrict);

            // 8. Loại túi — Túi xách: 1-N (Restrict - không cho xóa loại nếu còn túi)
            modelBuilder.Entity<TuiXach>()
                .HasOne(tx => tx.LoaiTui)
                .WithMany(lt => lt.TuiXachs)
                .HasForeignKey(tx => tx.maLoai)
                .OnDelete(DeleteBehavior.Restrict);

            // 9. Khuyến mãi — Túi xách: 1-N (SetNull - xóa khuyến mãi thì túi vẫn còn, chỉ gán null)
            modelBuilder.Entity<TuiXach>()
                .HasOne(tx => tx.KhuyenMai)
                .WithMany(km => km.TuiXachs)
                .HasForeignKey(tx => tx.maKM)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
