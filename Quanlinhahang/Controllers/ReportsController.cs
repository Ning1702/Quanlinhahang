using Microsoft.AspNetCore.Mvc;
using Quanlinhahang.Data;
using Quanlinhahang.Models.ViewModels;
using System;
using System.Linq;

namespace Quanlinhahang.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Reports
        public IActionResult Index()
        {
            // Giả sử nhân viên đăng nhập có ID = 1 (bạn có thể thay đổi sau khi có chức năng đăng nhập)
            int currentNhanVienId = 1;

            DateTime today = DateTime.Today;
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            // Tổng hóa đơn trong ngày
            var totalToday = _context.HoaDons
                .Count(h => h.NgayLap.Date == today.Date && h.TaiKhoanID == currentNhanVienId);

            // Tổng hóa đơn trong tháng
            var totalMonth = _context.HoaDons
                .Count(h => h.NgayLap >= firstDayOfMonth && h.TaiKhoanID == currentNhanVienId);

            // Tổng doanh thu (chỉ tính đã thanh toán)
            var doanhThu = _context.HoaDons
                .Where(h => h.TrangThaiThanhToan == "Đã thanh toán" && h.TaiKhoanID == currentNhanVienId)
                .Sum(h => (decimal?)h.TongTien) ?? 0;

            // Hoa hồng: 5% doanh thu
            var hoaHong = doanhThu * 0.05m;

            // Hóa đơn đã thanh toán
            var daTT = _context.HoaDons
                .Count(h => h.TrangThaiThanhToan == "Đã thanh toán" && h.TaiKhoanID == currentNhanVienId);

            // Hóa đơn chưa thanh toán
            var chuaTT = _context.HoaDons
                .Count(h => h.TrangThaiThanhToan != "Đã thanh toán" && h.TaiKhoanID == currentNhanVienId);

            var vm = new ReportVM
            {
                TongHoaDonHomNay = totalToday,
                TongHoaDonThangNay = totalMonth,
                TongDoanhThu = doanhThu,
                HoaHong = hoaHong,
                SoHoaDonDaThanhToan = daTT,
                SoHoaDonChuaThanhToan = chuaTT
            };

            return View(vm);
        }
    }
}
