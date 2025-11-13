using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlinhahang.Models
{
    [Table("KhungGio")]
    public class KhungGio
    {
        public int KhungGioID { get; set; }
        public string TenKhungGio { get; set; } = "";
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
    }
}
