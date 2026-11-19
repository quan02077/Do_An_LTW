using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnLTW.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutOptionsToDonHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ghiChu",
                table: "DonHangs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phuongThucThanhToan",
                table: "DonHangs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sdtNguoiNhan",
                table: "DonHangs",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tenNguoiNhan",
                table: "DonHangs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // đơn hàng tạo trước khi có các cột này: mặc định COD và lấy người nhận từ hồ sơ khách hàng,
            // nếu không, giá trị rỗng sẽ vi phạm CHECK constraint thêm ngay bên dưới.
            migrationBuilder.Sql(@"
UPDATE d
SET d.phuongThucThanhToan = N'COD',
    d.tenNguoiNhan = kh.hoTen,
    d.sdtNguoiNhan = ISNULL(kh.sdt, N'')
FROM [DonHangs] d
JOIN [KhachHangs] kh ON kh.maKH = d.maKH;");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonHang_phuongThucThanhToan",
                table: "DonHangs",
                sql: "[phuongThucThanhToan] IN (N'COD', N'ChuyenKhoan')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DonHang_phuongThucThanhToan",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "ghiChu",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "phuongThucThanhToan",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "sdtNguoiNhan",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "tenNguoiNhan",
                table: "DonHangs");
        }
    }
}
