using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlinhahang.Models
{
    [Table("HoaDon")]
    public class HoaDon
    {
        // SỬA LỖI: Thêm constructor để khởi tạo ICollection
        public HoaDon()
        {
            ChiTietHoaDons = new HashSet<ChiTietHoaDon>();
        }

        public int HoaDonID { get; set; }
        public int DatBanID { get; set; } // Khóa ngoại
        public int? BanPhongID { get; set; } // Khóa ngoại
        public int? TaiKhoanID { get; set; }
        public DateTime NgayLap { get; set; }
        public decimal TongTien { get; set; }

        // Bổ sung các trường đã mất từ lần trước
        public decimal GiamGia { get; set; }
        public int DiemCong { get; set; }
        public int DiemSuDung { get; set; }
        public string? HinhThucThanhToan { get; set; }
        public int TrangThaiID { get; set; }
        public decimal? VAT { get; set; }
        public string? LoaiDichVu { get; set; }


        // SỬA LỖI DatBanID1: Thêm [ForeignKey]
        [ForeignKey("DatBanID")]
        public virtual DatBan DatBan { get; set; }

        // SỬA LỖI BanPhongID1: Thêm [ForeignKey]
        [ForeignKey("BanPhongID")]
        public virtual BanPhong? BanPhong { get; set; }

        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual TrangThaiHoaDon TrangThai { get; set; }
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}