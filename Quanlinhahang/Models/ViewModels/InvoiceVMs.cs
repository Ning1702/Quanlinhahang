using System.ComponentModel.DataAnnotations;

namespace Quanlinhahang.Models.ViewModels
{
    // 🔍 Lọc hóa đơn theo trạng thái, ngày, từ khóa
    public class InvoiceFilterVM
    {
        public string? Search { get; set; }

        // ===== ĐÃ XÓA 2 DÒNG TRẠNG THÁI CŨ =====
        // public string? TrangThaiThanhToan { get; set; } 
        // public string? TrangThaiXacNhan { get; set; }

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

        // ===== ĐÃ SỬA =====
        public string TrangThai { get; set; } = "";

        // ===== ĐÃ XÓA 2 DÒNG TRẠNG THÁI CŨ =====
        // public string TrangThaiThanhToan { get; set; } = "";
        // public string TrangThaiXacNhan { get; set; } = "";
    }

    // ✏️ ViewModel chỉnh sửa / tạo hóa đơn
    public class InvoiceEditVM
    {
        public int HoaDonID { get; set; }
        public int DatBanID { get; set; }

        [Display(Name = "Giảm giá")]
        public decimal GiamGia { get; set; }

        [Display(Name = "Điểm sử dụng")]
        public int DiemSuDung { get; set; }

        [Display(Name = "Hình thức thanh toán")]
        public string? HinhThucThanhToan { get; set; }

        // ===== ĐÃ SỬA (Giữ nguyên) =====
        public string TrangThai { get; set; } = "";

        // ===== ĐÃ XÓA (Giữ nguyên) =====
        // public string? TrangThaiThanhToan { get; set; }
        // public string? TrangThaiXacNhan { get; set; } 


        public List<ItemLine> Items { get; set; } = new List<ItemLine>();

        // Lớp con ItemLine giữ nguyên
        public class ItemLine
        {
            public int MonAnID { get; set; }
            public string TenMon { get; set; } = "";
            public int SoLuong { get; set; }
            public decimal DonGia { get; set; }

            // Thêm thuộc tính tính toán này để Edit.cshtml dễ sử dụng
            public decimal ThanhTien => SoLuong * DonGia;
        }
    }
}