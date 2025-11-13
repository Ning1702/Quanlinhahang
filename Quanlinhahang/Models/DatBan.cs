using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlinhahang.Models
{
    [Table("DatBan")]
    public class DatBan
    {
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

        // ===== Navigation =====
        public KhachHang? KhachHang { get; set; }
        public BanPhong? BanPhong { get; set; }
        public KhungGio? KhungGio { get; set; }
    }
}
