using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlinhahang.Models
{
    [Table("MonAn")]
    public class MonAn
    {
        public int MonAnID { get; set; }
        public int DanhMucID { get; set; }
        public string TenMon { get; set; } = null!;
        public string? MoTa { get; set; }
        public decimal DonGia { get; set; }
        public string? LoaiMon { get; set; }
        public string? HinhAnhURL { get; set; }
        public string TrangThai { get; set; } = "Còn bán";
    }
}
