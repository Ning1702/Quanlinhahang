using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlinhahang.Models
{
    [Table("KhachHang")]
    public class KhachHang
    {
        public int KhachHangID { get; set; }
        public string HoTen { get; set; } = null!;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public int? HangThanhVienID { get; set; }
        public int? DiemTichLuy { get; set; }
    }
}
