using System.ComponentModel.DataAnnotations;

namespace Quanlinhahang.Models.ViewModels
{
    // 🔍 Lọc hóa đơn theo trạng thái, ngày, từ khóa
    public class InvoiceFilterVM
    {
        public string? Search { get; set; }
        public string? TrangThaiThanhToan { get; set; } // "Chưa thanh toán" / "Đã thanh toán" / null
        public string? TrangThaiXacNhan { get; set; }   // ✅ Thêm mới

        [DataType(DataType.Date)]
        public DateTime? From { get; set; }

        [DataType(DataType.Date)]
        public DateTime? To { get; set; }
    }

    // 📋 Hàng hiển thị trong danh sách hóa đơn
    public class InvoiceRowVM
    {
        public int HoaDonID { get; set; }
        public DateTime NgayLap { get; set; }
        public string KhachHang { get; set; } = "";
        public string? SoDienThoai { get; set; }
        public decimal TongTien { get; set; }

        public string TrangThaiThanhToan { get; set; } = "";
        public string TrangThaiXacNhan { get; set; } = "";  // ✅ Thêm mới
    }

    // ✏️ ViewModel chỉnh sửa / tạo hóa đơn
    public class InvoiceEditVM
    {
        public int HoaDonID { get; set; }
        public int DatBanID { get; set; }
        public decimal GiamGia { get; set; }
        public int DiemSuDung { get; set; }
        public string? HinhThucThanhToan { get; set; }

        public string TrangThaiThanhToan { get; set; } = "Chưa thanh toán";
        public string TrangThaiXacNhan { get; set; } = "Chưa xác nhận";  // ✅ Thêm mới

        public List<ItemLine> Items { get; set; } = new();

        // 📦 Chi tiết từng món ăn trong hóa đơn
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
