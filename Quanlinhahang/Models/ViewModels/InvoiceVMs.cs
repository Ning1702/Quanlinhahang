using System.ComponentModel.DataAnnotations;

namespace Quanlinhahang.Models.ViewModels
{
    public class InvoiceFilterVM
    {
        public string? Search { get; set; }
        public string? TrangThaiThanhToan { get; set; } // "Chưa thanh toán" / "Đã thanh toán" / null
        [DataType(DataType.Date)]
        public DateTime? From { get; set; }
        [DataType(DataType.Date)]
        public DateTime? To { get; set; }
    }

    public class InvoiceRowVM
    {
        public int HoaDonID { get; set; }
        public DateTime NgayLap { get; set; }
        public string KhachHang { get; set; } = "";
        public string? SoDienThoai { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThaiThanhToan { get; set; } = "";
    }

    public class InvoiceEditVM
    {
        public int HoaDonID { get; set; }
        public int DatBanID { get; set; }
        public decimal GiamGia { get; set; }
        public int DiemSuDung { get; set; }
        public string? HinhThucThanhToan { get; set; }
        public string TrangThaiThanhToan { get; set; } = "Chưa thanh toán";

        public List<ItemLine> Items { get; set; } = new();

        public class ItemLine
        {
            public int MonAnID { get; set; }
            public string TenMon { get; set; } = "";
            public int SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien => SoLuong * DonGia;
        }
    }
}
