using Microsoft.EntityFrameworkCore;
using Quanlinhahang.Models;

namespace Quanlinhahang.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
        public DbSet<DatBan> DatBans => Set<DatBan>();
        public DbSet<BanPhong> BanPhongs => Set<BanPhong>();
        public DbSet<LoaiBanPhong> LoaiBanPhongs => Set<LoaiBanPhong>();
        public DbSet<KhungGio> KhungGios => Set<KhungGio>();
        public DbSet<HoaDon> HoaDons => Set<HoaDon>();
        public DbSet<ChiTietHoaDon> ChiTietHoaDons => Set<ChiTietHoaDon>();
        public DbSet<MonAn> MonAns => Set<MonAn>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // === CHI TIẾT HÓA ĐƠN ===
            mb.Entity<ChiTietHoaDon>()
              .HasKey(ct => new { ct.HoaDonID, ct.MonAnID });

            mb.Entity<ChiTietHoaDon>()
              .HasOne(ct => ct.HoaDon)
              .WithMany(h => h.ChiTiet)
              .HasForeignKey(ct => ct.HoaDonID)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ChiTietHoaDon>()
              .HasOne(ct => ct.MonAn)
              .WithMany()
              .HasForeignKey(ct => ct.MonAnID)
              .OnDelete(DeleteBehavior.Restrict);

            // === HÓA ĐƠN – ĐẶT BÀN ===
            mb.Entity<HoaDon>()
              .HasOne(h => h.DatBan)
              .WithMany()
              .HasForeignKey(h => h.DatBanID)
              .OnDelete(DeleteBehavior.Restrict);

            // === ĐẶT BÀN – KHÁCH HÀNG ===
            mb.Entity<DatBan>()
              .HasOne(db => db.KhachHang)
              .WithMany()
              .HasForeignKey(db => db.KhachHangID)
              .OnDelete(DeleteBehavior.Restrict);

            // === ĐẶT BÀN – BÀN PHÒNG ===
            mb.Entity<DatBan>()
              .HasOne(db => db.BanPhong)
              .WithMany()
              .HasForeignKey(db => db.BanPhongID)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<BanPhong>()
                .HasOne(bp => bp.LoaiBanPhong)
                .WithMany(lp => lp.BanPhongs)
                .HasForeignKey(bp => bp.LoaiBanPhongID)
                .OnDelete(DeleteBehavior.Restrict);

            // === ĐẶT BÀN – KHUNG GIỜ ===
            mb.Entity<DatBan>()
              .HasOne(db => db.KhungGio)
              .WithMany()
              .HasForeignKey(db => db.KhungGioID)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
