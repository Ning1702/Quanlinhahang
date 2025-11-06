using Microsoft.EntityFrameworkCore;
using Quanlinhahang.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Quanlinhahang.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
        public DbSet<DatBan> DatBans => Set<DatBan>();
        public DbSet<HoaDon> HoaDons => Set<HoaDon>();
        public DbSet<ChiTietHoaDon> ChiTietHoaDons => Set<ChiTietHoaDon>();
        public DbSet<MonAn> MonAns => Set<MonAn>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // Khóa chính phức hợp CTHD
            mb.Entity<ChiTietHoaDon>()
              .HasKey(x => new { x.HoaDonID, x.MonAnID });

            // Quan hệ
            mb.Entity<HoaDon>()
              .HasOne(h => h.DatBan)
              .WithMany()
              .HasForeignKey(h => h.DatBanID)
              .OnDelete(DeleteBehavior.Restrict);

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
        }
    }
}
