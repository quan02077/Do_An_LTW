using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnLTW.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderAndInventoryConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_TuiXach_SoLuong",
                table: "TuiXachs",
                sql: "[soLuong] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonHang_TongTien",
                table: "DonHangs",
                sql: "[tongTien] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonHang_TrangThai",
                table: "DonHangs",
                sql: "[trangThai] IN (N'Chờ duyệt', N'Đang giao', N'Hoàn tất', N'Đã hủy')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChiTietGioHang_SoLuong",
                table: "ChiTietGioHangs",
                sql: "[soLuong] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChiTietDonHang_SoLuong",
                table: "ChiTietDonHangs",
                sql: "[soLuong] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TuiXach_SoLuong",
                table: "TuiXachs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonHang_TongTien",
                table: "DonHangs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonHang_TrangThai",
                table: "DonHangs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChiTietGioHang_SoLuong",
                table: "ChiTietGioHangs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChiTietDonHang_SoLuong",
                table: "ChiTietDonHangs");
        }
    }
}
