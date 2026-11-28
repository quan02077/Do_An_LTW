using Microsoft.EntityFrameworkCore;
using DoAnLTW.Models;
using DoAnLTW.Services;

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
        public DbSet<CuocTroChuyen> CuocTroChuyens { get; set; }
        public DbSet<TinNhan> TinNhans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureCheckConstraints(modelBuilder);

            // tài khoản - khách hàng: 1-1
            modelBuilder.Entity<KhachHang>()
                .HasOne(kh => kh.TaiKhoan)
                .WithOne(tk => tk.KhachHang)
                .HasForeignKey<KhachHang>(kh => kh.maTK);

            // khách hàng - giỏ hàng: 1-1
            modelBuilder.Entity<GioHang>()
                .HasOne(gh => gh.KhachHang)
                .WithOne(kh => kh.GioHang)
                .HasForeignKey<GioHang>(gh => gh.maKH);

            // giỏ hàng - chi tiết giỏ hàng: 1-n (composite key)
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

            // túi xách - hình ảnh: 1-n
            modelBuilder.Entity<HinhAnhTui>()
                .HasOne(ha => ha.TuiXach)
                .WithMany(tx => tx.HinhAnhTuis)
                .HasForeignKey(ha => ha.maTui);

            // khách hàng - đơn hàng: 1-n
            modelBuilder.Entity<DonHang>()
                .HasOne(dh => dh.KhachHang)
                .WithMany(kh => kh.DonHangs)
                .HasForeignKey(dh => dh.maKH);

            // đơn hàng - chi tiết đơn hàng: 1-n (khóa chính kết hợp)
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

            // thương hiệu - túi xách: 1-n
            modelBuilder.Entity<TuiXach>()
                .HasOne(tx => tx.ThuongHieu)
                .WithMany(th => th.TuiXachs)
                .HasForeignKey(tx => tx.maThuongHieu);

            // loại túi - túi xách: 1-n
            modelBuilder.Entity<TuiXach>()
                .HasOne(tx => tx.LoaiTui)
                .WithMany(lt => lt.TuiXachs)
                .HasForeignKey(tx => tx.maLoai);

            // khuyến mãi - túi xách: 1-n
            modelBuilder.Entity<TuiXach>()
                .HasOne(tx => tx.KhuyenMai)
                .WithMany(km => km.TuiXachs)
                .HasForeignKey(tx => tx.maKM)
                .IsRequired(false);

            // khách hàng - cuộc trò chuyện với nhân viên: 1-1 (mỗi khách đúng một cuộc)
            modelBuilder.Entity<CuocTroChuyen>()
                .HasOne(c => c.KhachHang)
                .WithOne()
                .HasForeignKey<CuocTroChuyen>(c => c.maKH);

            // cuộc trò chuyện - tin nhắn: 1-n; tra cứu theo cuộc trò chuyện và thời gian
            modelBuilder.Entity<TinNhan>()
                .HasOne(t => t.CuocTroChuyen)
                .WithMany(c => c.TinNhans)
                .HasForeignKey(t => t.maCTC);

            modelBuilder.Entity<TinNhan>()
                .HasIndex(t => new { t.maCTC, t.guiLuc });

            // nhân viên đã trả lời: giữ liên kết tới tài khoản, không cho xóa tài khoản làm mất lịch sử
            modelBuilder.Entity<TinNhan>()
                .HasOne<TaiKhoan>()
                .WithMany()
                .HasForeignKey(t => t.maTKNhanVien)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // ràng buộc CHECK ở tầng CSDL: lớp bảo vệ cuối cùng, kể cả khi code C# có lỗi
        // hoặc dữ liệu được sửa trực tiếp bằng SQL.
        private static void ConfigureCheckConstraints(ModelBuilder modelBuilder)
        {
            // tiền tố N'...' bắt buộc để SQL Server so sánh chuỗi Unicode (có dấu tiếng Việt).
            var trangThaiHopLe = string.Join(", ", new[]
            {
                OrderStatus.ChoDuyet, OrderStatus.DangGiao, OrderStatus.HoanTat, OrderStatus.DaHuy
            }.Select(s => $"N'{s}'"));

            modelBuilder.Entity<TuiXach>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_TuiXach_soLuong", "[soLuong] >= 0");
                t.HasCheckConstraint("CK_TuiXach_donGia", "[donGia] >= 0");
            });

            modelBuilder.Entity<ChiTietGioHang>().ToTable(t =>
                t.HasCheckConstraint("CK_ChiTietGioHang_soLuong", "[soLuong] > 0"));

            modelBuilder.Entity<DonHang>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_DonHang_tongTien", "[tongTien] >= 0");
                t.HasCheckConstraint("CK_DonHang_trangThai", $"[trangThai] IN ({trangThaiHopLe})");
                t.HasCheckConstraint("CK_DonHang_phuongThucThanhToan",
                    $"[phuongThucThanhToan] IN (N'{PaymentMethod.COD}', N'{PaymentMethod.ChuyenKhoan}')");
            });

            // tin nhắn không được rỗng (chỉ khoảng trắng) ở tầng CSDL
            modelBuilder.Entity<TinNhan>().ToTable(t =>
                t.HasCheckConstraint("CK_TinNhan_noiDung", "LEN(LTRIM(RTRIM([noiDung]))) > 0"));

            modelBuilder.Entity<ChiTietDonHang>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_ChiTietDonHang_soLuong", "[soLuong] > 0");
                t.HasCheckConstraint("CK_ChiTietDonHang_donGia", "[donGia] >= 0");
                t.HasCheckConstraint("CK_ChiTietDonHang_thanhTien", "[thanhTien] >= 0");
            });
        }
    }
}
