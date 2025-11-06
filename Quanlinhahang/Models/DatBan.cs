using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlinhahang.Models
{
    [Table("DatBan")]
    public class DatBan
    {
        public int DatBanID { get; set; }
        public int KhachHangID { get; set; }
        public int? BanPhongID { get; set; }
        public int KhungGioID { get; set; }
        public DateTime NgayDen { get; set; }     // DATE trong DB
        public int SoNguoi { get; set; }
        public decimal? TongTienDuKien { get; set; }
        public string? YeuCauDacBiet { get; set; }
        public string TrangThai { get; set; } = "Chờ xác nhận"; // Chờ xác nhận / Xác nhận / Hủy
        public DateTime NgayTao { get; set; }
    }
}
