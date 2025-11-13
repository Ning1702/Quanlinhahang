using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlinhahang.Models
{
    [Table("BanPhong")]
    public class BanPhong
    {
        public int BanPhongID { get; set; }
        public int LoaiBanPhongID { get; set; }
        public string TenBanPhong { get; set; } = "";
        public int SucChua { get; set; }
        public string TrangThai { get; set; } = "Trống";
    }
}
