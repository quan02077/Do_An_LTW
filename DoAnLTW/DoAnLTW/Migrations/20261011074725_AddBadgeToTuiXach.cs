using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnLTW.Migrations
{
    /// <inheritdoc />
    public partial class AddBadgeToTuiXach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "badge",
                table: "TuiXachs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TuiXach_Badge",
                table: "TuiXachs",
                sql: "[badge] IS NULL OR [badge] IN (N'Đang giảm giá (Sale)', N'Bán chạy (Best Seller)', N'Hàng mới về (New Arrival)', N'Sale', N'Best Seller', N'New Arrival')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TuiXach_Badge",
                table: "TuiXachs");

            migrationBuilder.DropColumn(
                name: "badge",
                table: "TuiXachs");
        }
    }
}
