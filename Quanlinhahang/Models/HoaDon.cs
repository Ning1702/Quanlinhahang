using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlinhahang.Models
{
    [Table("HoaDon")]
    public class HoaDon
    {
        public int HoaDonID { get; set; }
        public int DatBanID { get; set; }
        public int? TaiKhoanID { get; set; }     // Nhân viên lập
        public DateTime NgayLap { get; set; }
        public decimal TongTien { get; set; }
        public decimal GiamGia { get; set; }
        public int DiemCong { get; set; }
        public int DiemSuDung { get; set; }
        public string? HinhThucThanhToan { get; set; } // Tiền mặt / Thẻ / QR...
        public string TrangThai { get; set; } = "Chưa xác nhận";

        public DatBan? DatBan { get; set; }
        public List<ChiTietHoaDon> ChiTiet { get; set; } = new();
    }
}
