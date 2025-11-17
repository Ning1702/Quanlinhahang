using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlinhahang.Models
{
    [Table("DatBan")]
    public class DatBan
    {
        // SỬA LỖI: Thêm constructor để khởi tạo ICollection
        public DatBan()
        {
            HoaDons = new HashSet<HoaDon>();
        }

        public int DatBanID { get; set; }
        public int? KhachHangID { get; set; }
        public int? BanPhongID { get; set; }
        public int KhungGioID { get; set; }
        public DateTime NgayDen { get; set; }
        public int SoNguoi { get; set; }
        public decimal? TongTienDuKien { get; set; }
        public string? YeuCauDacBiet { get; set; }
        public string TrangThai { get; set; } = "Chờ xác nhận";
        public DateTime NgayTao { get; set; }

        [ForeignKey("BanPhongID")]
        public virtual BanPhong? BanPhong { get; set; }

        public virtual KhachHang? KhachHang { get; set; } // Thêm ?
        public virtual KhungGio KhungGio { get; set; }

        // SỬA LỖI DatBanID1: Thêm [InverseProperty]
        [InverseProperty("DatBan")]
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}