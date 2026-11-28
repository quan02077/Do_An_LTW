using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnLTW.Migrations
{
    /// <inheritdoc />
    public partial class ThemTroChuyenNhanVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuocTroChuyens",
                columns: table => new
                {
                    maCTC = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    maKH = table.Column<int>(type: "int", nullable: false),
                    capNhatLuc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuocTroChuyens", x => x.maCTC);
                    table.ForeignKey(
                        name: "FK_CuocTroChuyens_KhachHangs_maKH",
                        column: x => x.maKH,
                        principalTable: "KhachHangs",
                        principalColumn: "maKH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TinNhans",
                columns: table => new
                {
                    maTN = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    maCTC = table.Column<int>(type: "int", nullable: false),
                    tuKhach = table.Column<bool>(type: "bit", nullable: false),
                    maTKNhanVien = table.Column<int>(type: "int", nullable: true),
                    noiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    guiLuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    daDoc = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhans", x => x.maTN);
                    table.CheckConstraint("CK_TinNhan_noiDung", "LEN(LTRIM(RTRIM([noiDung]))) > 0");
                    table.ForeignKey(
                        name: "FK_TinNhans_CuocTroChuyens_maCTC",
                        column: x => x.maCTC,
                        principalTable: "CuocTroChuyens",
                        principalColumn: "maCTC",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TinNhans_TaiKhoans_maTKNhanVien",
                        column: x => x.maTKNhanVien,
                        principalTable: "TaiKhoans",
                        principalColumn: "maTK",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyens_maKH",
                table: "CuocTroChuyens",
                column: "maKH",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TinNhans_maCTC_guiLuc",
                table: "TinNhans",
                columns: new[] { "maCTC", "guiLuc" });

            migrationBuilder.CreateIndex(
                name: "IX_TinNhans_maTKNhanVien",
                table: "TinNhans",
                column: "maTKNhanVien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TinNhans");

            migrationBuilder.DropTable(
                name: "CuocTroChuyens");
        }
    }
}
