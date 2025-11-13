using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quanlinhahang.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BanPhong",
                columns: table => new
                {
                    BanPhongID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenBanPhong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiBanPhongID = table.Column<int>(type: "int", nullable: false),
                    SucChua = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanPhong", x => x.BanPhongID);
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    KhachHangID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HangThanhVienID = table.Column<int>(type: "int", nullable: true),
                    DiemTichLuy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.KhachHangID);
                });

            migrationBuilder.CreateTable(
                name: "KhungGio",
                columns: table => new
                {
                    KhungGioID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhungGio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GioBatDau = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhungGio", x => x.KhungGioID);
                });

            migrationBuilder.CreateTable(
                name: "MonAn",
                columns: table => new
                {
                    MonAnID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DanhMucID = table.Column<int>(type: "int", nullable: false),
                    TenMon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoaiMon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnhURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonAn", x => x.MonAnID);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    TaiKhoanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.TaiKhoanID);
                });

            migrationBuilder.CreateTable(
                name: "DatBan",
                columns: table => new
                {
                    DatBanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KhachHangID = table.Column<int>(type: "int", nullable: false),
                    BanPhongID = table.Column<int>(type: "int", nullable: true),
                    KhungGioID = table.Column<int>(type: "int", nullable: false),
                    NgayDen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoNguoi = table.Column<int>(type: "int", nullable: false),
                    TongTienDuKien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    YeuCauDacBiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BanPhongID1 = table.Column<int>(type: "int", nullable: true),
                    KhungGioID1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatBan", x => x.DatBanID);
                    table.ForeignKey(
                        name: "FK_DatBan_BanPhong_BanPhongID",
                        column: x => x.BanPhongID,
                        principalTable: "BanPhong",
                        principalColumn: "BanPhongID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatBan_BanPhong_BanPhongID1",
                        column: x => x.BanPhongID1,
                        principalTable: "BanPhong",
                        principalColumn: "BanPhongID");
                    table.ForeignKey(
                        name: "FK_DatBan_KhachHang_KhachHangID",
                        column: x => x.KhachHangID,
                        principalTable: "KhachHang",
                        principalColumn: "KhachHangID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatBan_KhungGio_KhungGioID",
                        column: x => x.KhungGioID,
                        principalTable: "KhungGio",
                        principalColumn: "KhungGioID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatBan_KhungGio_KhungGioID1",
                        column: x => x.KhungGioID1,
                        principalTable: "KhungGio",
                        principalColumn: "KhungGioID");
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    HoaDonID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatBanID = table.Column<int>(type: "int", nullable: false),
                    TaiKhoanID = table.Column<int>(type: "int", nullable: true),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiamGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiemCong = table.Column<int>(type: "int", nullable: false),
                    DiemSuDung = table.Column<int>(type: "int", nullable: false),
                    HinhThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatBanID1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.HoaDonID);
                    table.ForeignKey(
                        name: "FK_HoaDon_DatBan_DatBanID",
                        column: x => x.DatBanID,
                        principalTable: "DatBan",
                        principalColumn: "DatBanID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDon_DatBan_DatBanID1",
                        column: x => x.DatBanID1,
                        principalTable: "DatBan",
                        principalColumn: "DatBanID");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDon",
                columns: table => new
                {
                    HoaDonID = table.Column<int>(type: "int", nullable: false),
                    MonAnID = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietHoaDon", x => new { x.HoaDonID, x.MonAnID });
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDon_HoaDon_HoaDonID",
                        column: x => x.HoaDonID,
                        principalTable: "HoaDon",
                        principalColumn: "HoaDonID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDon_MonAn_MonAnID",
                        column: x => x.MonAnID,
                        principalTable: "MonAn",
                        principalColumn: "MonAnID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDon_MonAnID",
                table: "ChiTietHoaDon",
                column: "MonAnID");

            migrationBuilder.CreateIndex(
                name: "IX_DatBan_BanPhongID",
                table: "DatBan",
                column: "BanPhongID");

            migrationBuilder.CreateIndex(
                name: "IX_DatBan_BanPhongID1",
                table: "DatBan",
                column: "BanPhongID1");

            migrationBuilder.CreateIndex(
                name: "IX_DatBan_KhachHangID",
                table: "DatBan",
                column: "KhachHangID");

            migrationBuilder.CreateIndex(
                name: "IX_DatBan_KhungGioID",
                table: "DatBan",
                column: "KhungGioID");

            migrationBuilder.CreateIndex(
                name: "IX_DatBan_KhungGioID1",
                table: "DatBan",
                column: "KhungGioID1");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_DatBanID",
                table: "HoaDon",
                column: "DatBanID");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_DatBanID1",
                table: "HoaDon",
                column: "DatBanID1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietHoaDon");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "MonAn");

            migrationBuilder.DropTable(
                name: "DatBan");

            migrationBuilder.DropTable(
                name: "BanPhong");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "KhungGio");
        }
    }
}
