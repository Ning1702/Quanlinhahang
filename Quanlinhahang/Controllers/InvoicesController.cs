using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quanlinhahang.Data;
using Quanlinhahang.Models;
using Quanlinhahang.Models.ViewModels;

namespace Quanlinhahang.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly AppDbContext _db;

        public InvoicesController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Invoices
        public async Task<IActionResult> Index([FromQuery] InvoiceFilterVM f)
        {
            var q = _db.HoaDons
                .Include(h => h.DatBan)
                .AsQueryable();

            // join để lấy KhachHang
            var data = from h in q
                       join d in _db.DatBans on h.DatBanID equals d.DatBanID
                       join k in _db.KhachHangs on d.KhachHangID equals k.KhachHangID
                       select new InvoiceRowVM
                       {
                           HoaDonID = h.HoaDonID,
                           NgayLap = h.NgayLap,
                           KhachHang = k.HoTen,
                           SoDienThoai = k.SoDienThoai,
                           TongTien = h.TongTien,
                           TrangThaiThanhToan = h.TrangThaiThanhToan
                       };

            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                string s = f.Search.Trim();
                data = data.Where(x => x.KhachHang.Contains(s) || (x.SoDienThoai ?? "").Contains(s));
            }
            if (!string.IsNullOrWhiteSpace(f.TrangThaiThanhToan))
            {
                data = data.Where(x => x.TrangThaiThanhToan == f.TrangThaiThanhToan);
            }
            if (f.From.HasValue) data = data.Where(x => x.NgayLap.Date >= f.From.Value.Date);
            if (f.To.HasValue) data = data.Where(x => x.NgayLap.Date <= f.To.Value.Date);

            var list = await data
                .OrderByDescending(x => x.NgayLap)
                .Take(500)
                .ToListAsync();

            ViewBag.Filter = f;
            return View(list);
        }

        // GET: /Invoices/Create?datBanId=123
        public async Task<IActionResult> Create(int? datBanId)
        {
            // tạo hóa đơn mới từ một đặt bàn đã "Xác nhận"
            if (datBanId == null)
                return BadRequest("Thiếu DatBanID");

            var datBan = await _db.DatBans.FirstOrDefaultAsync(x => x.DatBanID == datBanId);
            if (datBan == null) return NotFound("Không tìm thấy lịch đặt.");

            // Tạo rỗng, nhân viên sẽ thêm món
            var hd = new HoaDon
            {
                DatBanID = datBan.DatBanID,
                NgayLap = DateTime.Now,
                TongTien = 0,
                GiamGia = 0,
                DiemCong = 0,
                DiemSuDung = 0,
                TrangThaiThanhToan = "Chưa thanh toán"
            };
            _db.HoaDons.Add(hd);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = hd.HoaDonID });
        }

        // GET: /Invoices/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var hd = await _db.HoaDons
                .Include(h => h.ChiTiet)
                .ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();

            var vm = new InvoiceEditVM
            {
                HoaDonID = hd.HoaDonID,
                DatBanID = hd.DatBanID,
                GiamGia = hd.GiamGia,
                DiemSuDung = hd.DiemSuDung,
                HinhThucThanhToan = hd.HinhThucThanhToan,
                TrangThaiThanhToan = hd.TrangThaiThanhToan,
                Items = hd.ChiTiet.Select(ct => new InvoiceEditVM.ItemLine
                {
                    MonAnID = ct.MonAnID,
                    TenMon = ct.MonAn?.TenMon ?? "",
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia
                }).ToList()
            };

            ViewBag.MonAn = await _db.MonAns
                .Where(m => m.TrangThai == "Còn bán")
                .OrderBy(m => m.TenMon)
                .ToListAsync();

            return View(vm);
        }

        // POST: /Invoices/AddItem
        [HttpPost]
        public async Task<IActionResult> AddItem(int hoaDonId, int monAnId, int soLuong = 1)
        {
            var mon = await _db.MonAns.FindAsync(monAnId);
            var hd = await _db.HoaDons.Include(h => h.ChiTiet).FirstOrDefaultAsync(h => h.HoaDonID == hoaDonId);
            if (mon == null || hd == null) return NotFound();

            var ct = await _db.ChiTietHoaDons.FindAsync(hoaDonId, monAnId);
            if (ct == null)
            {
                ct = new ChiTietHoaDon
                {
                    HoaDonID = hoaDonId,
                    MonAnID = monAnId,
                    SoLuong = soLuong,
                    DonGia = mon.DonGia,
                    ThanhTien = mon.DonGia * soLuong
                };
                _db.ChiTietHoaDons.Add(ct);
            }
            else
            {
                ct.SoLuong += soLuong;
                ct.ThanhTien = ct.SoLuong * ct.DonGia;
                _db.ChiTietHoaDons.Update(ct);
            }

            await UpdateTongTienAsync(hoaDonId);
            return RedirectToAction(nameof(Edit), new { id = hoaDonId });
        }

        // POST: /Invoices/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int hoaDonId, int monAnId)
        {
            var ct = await _db.ChiTietHoaDons.FindAsync(hoaDonId, monAnId);
            if (ct != null)
            {
                _db.ChiTietHoaDons.Remove(ct);
                await _db.SaveChangesAsync();
                await UpdateTongTienAsync(hoaDonId);
            }
            return RedirectToAction(nameof(Edit), new { id = hoaDonId });
        }

        // POST: /Invoices/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(InvoiceEditVM vm)
        {
            var hd = await _db.HoaDons.FindAsync(vm.HoaDonID);
            if (hd == null) return NotFound();

            hd.GiamGia = vm.GiamGia;
            hd.DiemSuDung = vm.DiemSuDung;
            hd.HinhThucThanhToan = vm.HinhThucThanhToan;
            hd.TrangThaiThanhToan = vm.TrangThaiThanhToan;

            await UpdateTongTienAsync(hd.HoaDonID);
            await _db.SaveChangesAsync();

            TempData["msg"] = "Đã lưu hóa đơn.";
            return RedirectToAction(nameof(Edit), new { id = hd.HoaDonID });
        }

        // POST: /Invoices/ConfirmPayment/5
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var hd = await _db.HoaDons.FindAsync(id);
            if (hd == null) return NotFound();

            hd.TrangThaiThanhToan = "Đã thanh toán";
            await _db.SaveChangesAsync();

            TempData["msg"] = "Đã xác nhận thanh toán.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Invoices/Print/5 (trang in -> Ctrl+P để lưu PDF)
        public async Task<IActionResult> Print(int id)
        {
            var hd = await _db.HoaDons
                .Include(h => h.DatBan)
                .Include(h => h.ChiTiet).ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();

            // lấy KH
            var kh = await (from d in _db.DatBans
                            join k in _db.KhachHangs on d.KhachHangID equals k.KhachHangID
                            where d.DatBanID == hd.DatBanID
                            select k).FirstOrDefaultAsync();

            ViewBag.KhachHang = kh;
            return View(hd);
        }

        private async Task UpdateTongTienAsync(int hoaDonId)
        {
            var hd = await _db.HoaDons
                .Include(h => h.ChiTiet)
                .FirstOrDefaultAsync(h => h.HoaDonID == hoaDonId);

            if (hd == null) return;

            var sum = hd.ChiTiet.Sum(x => x.ThanhTien);
            // Áp giảm giá và điểm (đơn giản)
            sum = sum - hd.GiamGia;
            if (sum < 0) sum = 0;
            hd.TongTien = sum;

            _db.HoaDons.Update(hd);
            await _db.SaveChangesAsync();
        }
    }
}
