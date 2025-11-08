using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quanlinhahang.Data;
using Quanlinhahang.Models;
using Quanlinhahang.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;

namespace Quanlinhahang.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly AppDbContext _context;

        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var list = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Select(h => new InvoiceRowVM
                {
                    HoaDonID = h.HoaDonID,
                    NgayLap = h.NgayLap,
                    KhachHang = h.KhachHang.TenKhachHang,
                    SoDienThoai = h.KhachHang.SoDienThoai,
                    TongTien = h.TongTien,
                    TrangThaiXacNhan = h.TrangThaiXacNhan,
                    TrangThaiThanhToan = h.TrangThaiThanhToan,
                    LoaiDatBan = h.LoaiDatBan
                })
                .ToListAsync();

            return View(list);
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(m => m.HoaDonID == id);

            if (hoaDon == null)
                return NotFound();

            return View(hoaDon);
        }

        // ✅ GET: Invoices/Watch/5
        // Xem chi tiết hóa đơn (chỉ đọc, không chỉnh sửa)
        public async Task<IActionResult> Watch(int? id)
        {
            if (id == null)
                return NotFound();

            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(m => m.HoaDonID == id);

            if (hoaDon == null)
                return NotFound();

            // Dùng lại view Details.cshtml để hiển thị
            return View("Details", hoaDon);
        }

        // GET: Invoices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hoaDon == null)
                return NotFound();

            return View(hoaDon);
        }

        // POST: Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HoaDon hoaDon)
        {
            if (id != hoaDon.HoaDonID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hoaDon);
                    await _context.SaveChangesAsync();
                    TempData["msg"] = "Cập nhật hóa đơn thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.HoaDons.Any(e => e.HoaDonID == hoaDon.HoaDonID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hoaDon);
        }

        // POST: Invoices/ConfirmInvoice/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmInvoice(int id)
        {
            var hoaDon = await _context.HoaDons.FindAsync(id);
            if (hoaDon == null)
                return NotFound();

            hoaDon.TrangThaiXacNhan = "Đã xác nhận";
            _context.Update(hoaDon);
            await _context.SaveChangesAsync();

            TempData["msg"] = $"Hóa đơn #{hoaDon.HoaDonID} đã được xác nhận.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Invoices/Print/5
        public async Task<IActionResult> Print(int? id)
        {
            if (id == null)
                return NotFound();

            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hoaDon == null)
                return NotFound();

            return View(hoaDon); // Views/Invoices/Print.cshtml
        }
    }
}
