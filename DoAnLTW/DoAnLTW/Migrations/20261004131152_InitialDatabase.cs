using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnLTW.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhuyenMais",
                columns: table => new
                {
                    maKM = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tenKM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phanTramGiam = table.Column<double>(type: "float", nullable: false),
                    ngayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    trangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMais", x => x.maKM);
                });

            migrationBuilder.CreateTable(
                name: "LoaiTuis",
                columns: table => new
                {
                    maLoai = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tenLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiTuis", x => x.maLoai);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    maTK = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    matKhau = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    vaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.maTK);
                });

            migrationBuilder.CreateTable(
                name: "ThuongHieus",
                columns: table => new
                {
                    maThuongHieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tenThuongHieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThuongHieus", x => x.maThuongHieu);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    maKH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hoTen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    sdt = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    diaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    matk = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.maKH);
                    table.ForeignKey(
                        name: "FK_KhachHangs_TaiKhoans_matk",
                        column: x => x.matk,
                        principalTable: "TaiKhoans",
                        principalColumn: "maTK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TuiXachs",
                columns: table => new
                {
                    maTui = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tenTui = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    maThuongHieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    maLoai = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    donGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    soLuong = table.Column<int>(type: "int", nullable: false),
                    moTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    maKM = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuiXachs", x => x.maTui);
                    table.ForeignKey(
                        name: "FK_TuiXachs_KhuyenMais_maKM",
                        column: x => x.maKM,
                        principalTable: "KhuyenMais",
                        principalColumn: "maKM",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TuiXachs_LoaiTuis_maLoai",
                        column: x => x.maLoai,
                        principalTable: "LoaiTuis",
                        principalColumn: "maLoai",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TuiXachs_ThuongHieus_maThuongHieu",
                        column: x => x.maThuongHieu,
                        principalTable: "ThuongHieus",
                        principalColumn: "maThuongHieu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonHangs",
                columns: table => new
                {
                    maDH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    maKH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    tongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    trangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    diaChiNhan = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangs", x => x.maDH);
                    table.ForeignKey(
                        name: "FK_DonHangs_KhachHangs_maKH",
                        column: x => x.maKH,
                        principalTable: "KhachHangs",
                        principalColumn: "maKH",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GioHangs",
                columns: table => new
                {
                    maGioHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    maKH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangs", x => x.maGioHang);
                    table.ForeignKey(
                        name: "FK_GioHangs_KhachHangs_maKH",
                        column: x => x.maKH,
                        principalTable: "KhachHangs",
                        principalColumn: "maKH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhTuis",
                columns: table => new
                {
                    maHinhAnh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    maTui = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tenHinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    laAnhChinh = table.Column<bool>(type: "bit", nullable: false),
                    thuTu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhTuis", x => x.maHinhAnh);
                    table.ForeignKey(
                        name: "FK_HinhAnhTuis_TuiXachs_maTui",
                        column: x => x.maTui,
                        principalTable: "TuiXachs",
                        principalColumn: "maTui",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHangs",
                columns: table => new
                {
                    maDH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    maTui = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    soLuong = table.Column<int>(type: "int", nullable: false),
                    donGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    thanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHangs", x => new { x.maDH, x.maTui });
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_DonHangs_maDH",
                        column: x => x.maDH,
                        principalTable: "DonHangs",
                        principalColumn: "maDH",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_TuiXachs_maTui",
                        column: x => x.maTui,
                        principalTable: "TuiXachs",
                        principalColumn: "maTui",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietGioHangs",
                columns: table => new
                {
                    maGioHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    maTui = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    soLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietGioHangs", x => new { x.maGioHang, x.maTui });
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_GioHangs_maGioHang",
                        column: x => x.maGioHang,
                        principalTable: "GioHangs",
                        principalColumn: "maGioHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_TuiXachs_maTui",
                        column: x => x.maTui,
                        principalTable: "TuiXachs",
                        principalColumn: "maTui",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_maTui",
                table: "ChiTietDonHangs",
                column: "maTui");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHangs_maTui",
                table: "ChiTietGioHangs",
                column: "maTui");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_maKH",
                table: "DonHangs",
                column: "maKH");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_maKH",
                table: "GioHangs",
                column: "maKH",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhTuis_maTui",
                table: "HinhAnhTuis",
                column: "maTui");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangs_matk",
                table: "KhachHangs",
                column: "matk",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuiXachs_maKM",
                table: "TuiXachs",
                column: "maKM");

            migrationBuilder.CreateIndex(
                name: "IX_TuiXachs_maLoai",
                table: "TuiXachs",
                column: "maLoai");

            migrationBuilder.CreateIndex(
                name: "IX_TuiXachs_maThuongHieu",
                table: "TuiXachs",
                column: "maThuongHieu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "ChiTietGioHangs");

            migrationBuilder.DropTable(
                name: "HinhAnhTuis");

            migrationBuilder.DropTable(
                name: "DonHangs");

            migrationBuilder.DropTable(
                name: "GioHangs");

            migrationBuilder.DropTable(
                name: "TuiXachs");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "KhuyenMais");

            migrationBuilder.DropTable(
                name: "LoaiTuis");

            migrationBuilder.DropTable(
                name: "ThuongHieus");

            migrationBuilder.DropTable(
                name: "TaiKhoans");
        }
    }
}
