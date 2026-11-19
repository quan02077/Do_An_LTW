using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnLTW.Migrations
{
    /// <inheritdoc />
    public partial class AddCartOrderCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // dọn dòng giỏ hàng sai (số lượng <= 0, do lỗi cập nhật cũ) để ADD CONSTRAINT không thất bại.
            // giỏ hàng là dữ liệu tạm nên xóa dòng sai là an toàn; dữ liệu đơn hàng thì KHÔNG tự sửa.
            migrationBuilder.Sql("DELETE FROM [ChiTietGioHangs] WHERE [soLuong] <= 0;");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TuiXach_donGia",
                table: "TuiXachs",
                sql: "[donGia] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TuiXach_soLuong",
                table: "TuiXachs",
                sql: "[soLuong] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonHang_tongTien",
                table: "DonHangs",
                sql: "[tongTien] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonHang_trangThai",
                table: "DonHangs",
                sql: "[trangThai] IN (N'Chờ duyệt', N'Đang giao', N'Hoàn tất', N'Đã hủy')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChiTietGioHang_soLuong",
                table: "ChiTietGioHangs",
                sql: "[soLuong] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChiTietDonHang_donGia",
                table: "ChiTietDonHangs",
                sql: "[donGia] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChiTietDonHang_soLuong",
                table: "ChiTietDonHangs",
                sql: "[soLuong] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChiTietDonHang_thanhTien",
                table: "ChiTietDonHangs",
                sql: "[thanhTien] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TuiXach_donGia",
                table: "TuiXachs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TuiXach_soLuong",
                table: "TuiXachs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonHang_tongTien",
                table: "DonHangs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonHang_trangThai",
                table: "DonHangs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChiTietGioHang_soLuong",
                table: "ChiTietGioHangs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChiTietDonHang_donGia",
                table: "ChiTietDonHangs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChiTietDonHang_soLuong",
                table: "ChiTietDonHangs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChiTietDonHang_thanhTien",
                table: "ChiTietDonHangs");
        }
    }
}
