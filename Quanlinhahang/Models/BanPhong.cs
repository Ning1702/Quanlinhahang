using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Quanlinhahang.Models
{
    [Table("BanPhong")]
    public class BanPhong
    {
        // SỬA LỖI: Thêm constructor
        public BanPhong()
        {
            DatBans = new HashSet<DatBan>();
            HoaDons = new HashSet<HoaDon>();
        }

        public int BanPhongID { get; set; }
        public int LoaiBanPhongID { get; set; }
        public string TenBanPhong { get; set; }
        public int SucChua { get; set; }
        public string TrangThai { get; set; }

        public virtual LoaiBanPhong LoaiBanPhong { get; set; }

        // =============================================
        // SỬA LỖI "BanPhongID1" TẠI ĐÂY
        [InverseProperty("BanPhong")]
        public virtual ICollection<DatBan> DatBans { get; set; }

        [InverseProperty("BanPhong")]
        public virtual ICollection<HoaDon> HoaDons { get; set; }
        // =============================================
    }
}