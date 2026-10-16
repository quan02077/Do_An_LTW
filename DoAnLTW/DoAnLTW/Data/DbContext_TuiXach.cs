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

            // Bỏ qua badge vì bảng tuixach trong quan_ly_tui_xach.sql không có cột này (badge được tính động trong logic C#)
            modelBuilder.Entity<TuiXach>()
                .Ignore(tx => tx.badge);

            // 1. Tài khoản — Khách hàng: 1-1
            modelBuilder.Entity<KhachHang>()
                .HasOne(kh => kh.TaiKhoan)
                .WithOne(tk => tk.KhachHang)
                .HasForeignKey<KhachHang>(kh => kh.matk);

            // 2. Khách hàng — Giỏ hàng: 1-1
            modelBuilder.Entity<GioHang>()
                .HasOne(gh => gh.KhachHang)
                .WithOne(kh => kh.GioHang)
                .HasForeignKey<GioHang>(gh => gh.maKH);

            // 3. Giỏ hàng — Chi tiết giỏ hàng: 1-N (Composite Key)
            modelBuilder.Entity<ChiTietGioHang>()
                .HasKey(ct => new { ct.maGioHang, ct.maTui });

            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(ct => ct.GioHang)
                .WithMany(gh => gh.ChiTietGioHangs)
                .HasForeignKey(ct => ct.maGioHang);

            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(ct => ct.TuiXach)
                .WithMany(tx => tx.ChiTietGioHangs)
                .HasForeignKey(ct => ct.maTui);

            // 4. Túi xách — Hình ảnh: 1-N
            modelBuilder.Entity<HinhAnhTui>()
                .HasOne(ha => ha.TuiXach)
                .WithMany(tx => tx.HinhAnhTuis)
                .HasForeignKey(ha => ha.maTui);

            // 5. Khách hàng — Đơn hàng: 1-N
            modelBuilder.Entity<DonHang>()
                .HasOne(dh => dh.KhachHang)
                .WithMany(kh => kh.DonHangs)
                .HasForeignKey(dh => dh.maKH);

            // 6. Đơn hàng — Chi tiết đơn hàng: 1-N (Composite Key)
            modelBuilder.Entity<ChiTietDonHang>()
                .HasKey(ct => new { ct.maDH, ct.maTui });

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DonHang)
                .WithMany(dh => dh.ChiTietDonHangs)
                .HasForeignKey(ct => ct.maDH);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.TuiXach)
                .WithMany(tx => tx.ChiTietDonHangs)
                .HasForeignKey(ct => ct.maTui);

            // 7. Thương hiệu — Túi xách: 1-N
            modelBuilder.Entity<TuiXach>()
                .HasOne(tx => tx.ThuongHieu)
                .WithMany(th => th.TuiXachs)
                .HasForeignKey(tx => tx.maThuongHieu);

            // 8. Loại túi — Túi xách: 1-N
            modelBuilder.Entity<TuiXach>()
                .HasOne(tx => tx.LoaiTui)
                .WithMany(lt => lt.TuiXachs)
                .HasForeignKey(tx => tx.maLoai);

            // 9. Khuyến mãi — Túi xách: 1-N
            modelBuilder.Entity<TuiXach>()
                .HasOne(tx => tx.KhuyenMai)
                .WithMany(km => km.TuiXachs)
                .HasForeignKey(tx => tx.maKM)
                .IsRequired(false);
        }
    }
}
