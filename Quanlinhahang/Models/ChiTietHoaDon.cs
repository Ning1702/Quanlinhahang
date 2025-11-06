using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlinhahang.Models
{
    [Table("ChiTietHoaDon")]
    public class ChiTietHoaDon
    {
        public int HoaDonID { get; set; }
        public int MonAnID { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }

        public HoaDon? HoaDon { get; set; }
        public MonAn? MonAn { get; set; }
    }
}
