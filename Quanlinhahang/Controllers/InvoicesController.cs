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

        // ==============================
        // LIST HÓA ĐƠN
        // ==============================
        public async Task<IActionResult> Index([FromQuery] InvoiceFilterVM f, [FromQuery] string status = "")
        {
            var q = _db.HoaDons.Include(h => h.DatBan).AsQueryable();

            var data =
                from h in q
                join d in _db.DatBans on h.DatBanID equals d.DatBanID
                join k in _db.KhachHangs on d.KhachHangID equals k.KhachHangID
                select new InvoiceRowVM
                {
                    HoaDonID = h.HoaDonID,
                    NgayLap = h.NgayLap,
                    KhachHang = k.HoTen,
                    SoDienThoai = k.SoDienThoai,
                    TongTien = h.TongTien,
                    TrangThai = h.TrangThai
                };

            ViewBag.Status = status;

            switch (status)
            {
                case "ChoXacNhan":
                    data = data.Where(x => x.TrangThai == "Chờ xác nhận" || x.TrangThai == "Chưa xác nhận");
                    break;
                case "DaXacNhan":
                    data = data.Where(x => x.TrangThai == "Đã xác nhận");
                    break;
                case "DangPhucVu":
                    data = data.Where(x => x.TrangThai == "Đang phục vụ");
                    break;
                case "DaThanhToan":
                    data = data.Where(x => x.TrangThai == "Đã thanh toán");
                    break;
                case "DaHuy":
                    data = data.Where(x => x.TrangThai == "Đã hủy");
                    break;
            }

            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                string s = f.Search.Trim();
                data = data.Where(x => x.KhachHang.Contains(s) || (x.SoDienThoai ?? "").Contains(s));
            }

            if (f.From.HasValue)
                data = data.Where(x => x.NgayLap.Date >= f.From.Value.Date);

            if (f.To.HasValue)
                data = data.Where(x => x.NgayLap.Date <= f.To.Value.Date);

            var list = await data.OrderByDescending(x => x.NgayLap).Take(500).ToListAsync();

            ViewBag.Filter = f;
            return View(list);
        }

        // ==============================
        // CHUYỂN SANG PHỤC VỤ
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartServing(int id, string currentStatus)
        {
            var hd = await _db.HoaDons.FindAsync(id);
            if (hd == null) return NotFound();

            if (hd.TrangThai == "Đã xác nhận")
            {
                hd.TrangThai = "Đang phục vụ";
                await _db.SaveChangesAsync();
                TempData["msg"] = "✅ Đã chuyển sang trạng thái phục vụ.";
            }
            else
            {
                TempData["msg"] = "⚠️ Không thể phục vụ hóa đơn này.";
            }

            return RedirectToAction(nameof(Index), new { status = currentStatus });
        }

        // ==============================
        // XÁC NHẬN HÓA ĐƠN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmInvoice(int id, string currentStatus)
        {
            var hd = await _db.HoaDons.FindAsync(id);
            if (hd == null) return NotFound();

            if (hd.TrangThai == "Chờ xác nhận" || hd.TrangThai == "Chưa xác nhận")
            {
                hd.TrangThai = "Đã xác nhận";
                await _db.SaveChangesAsync();
                TempData["msg"] = "✅ Hóa đơn đã được xác nhận.";
            }
            else
            {
                TempData["msg"] = "⚠️ Không thể xác nhận hóa đơn này.";
            }

            return RedirectToAction(nameof(Index), new { status = currentStatus });
        }

        // ==============================
        // HỦY HÓA ĐƠN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyHoaDon(int id, string currentStatus)
        {
            var hd = await _db.HoaDons.FindAsync(id);
            if (hd == null) return NotFound();

            if (hd.TrangThai != "Đã thanh toán")
            {
                hd.TrangThai = "Đã hủy";
                await _db.SaveChangesAsync();
                TempData["msg"] = "🗑 Hóa đơn đã bị hủy.";
            }
            else TempData["msg"] = "⚠️ Không thể hủy hóa đơn đã thanh toán.";

            return RedirectToAction(nameof(Index), new { status = currentStatus });
        }

        // ==============================
        // THANH TOÁN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(int id, string currentStatus)
        {
            var hd = await _db.HoaDons.Include(h => h.ChiTiet).FirstOrDefaultAsync(h => h.HoaDonID == id);
            if (hd == null) return NotFound();

            if (hd.TrangThai == "Đang phục vụ")
            {
                await UpdateTongTienAsync(hd);
                hd.TrangThai = "Đã thanh toán";
                await _db.SaveChangesAsync();
                TempData["msg"] = "💰 Đã thanh toán.";
            }
            else TempData["msg"] = "⚠️ Chỉ có thể thanh toán hóa đơn đang phục vụ.";

            return RedirectToAction(nameof(Index), new { status = currentStatus });
        }

        // ==============================
        // TẠO HÓA ĐƠN
        // ==============================
        public async Task<IActionResult> Create(int? datBanId)
        {
            if (datBanId == null) return BadRequest("Thiếu DatBanID");

            var datBan = await _db.DatBans.FindAsync(datBanId);
            if (datBan == null) return NotFound();

            var hd = new HoaDon
            {
                DatBanID = datBan.DatBanID,
                NgayLap = DateTime.Now,
                TongTien = 0,
                TrangThai = "Chờ xác nhận"
            };

            _db.HoaDons.Add(hd);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = hd.HoaDonID });
        }

        // ==============================
        // EDIT HÓA ĐƠN
        // ==============================
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.RefererUrl = Request.Headers["Referer"].ToString();

            var hd = await _db.HoaDons
                .Include(h => h.ChiTiet).ThenInclude(ct => ct.MonAn)
                .Include(h => h.DatBan)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();

            var vm = new InvoiceEditVM
            {
                HoaDonID = hd.HoaDonID,
                DatBanID = hd.DatBanID,
                BanPhongID = hd.DatBan?.BanPhongID,
                GiamGia = hd.GiamGia,
                DiemSuDung = hd.DiemSuDung,
                HinhThucThanhToan = hd.HinhThucThanhToan,
                TrangThai = hd.TrangThai,
                Items = hd.ChiTiet.Select(ct => new InvoiceEditVM.ItemLine
                {
                    MonAnID = ct.MonAnID,
                    TenMon = ct.MonAn.TenMon,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia
                }).ToList()
            };

            ViewBag.MonAn = await _db.MonAns.Where(m => m.TrangThai == "Còn bán").ToListAsync();
            ViewBag.BanPhongs = await _db.BanPhongs.ToListAsync();
            ViewBag.TrangThai = hd.TrangThai;
            ViewBag.DaThanhToan = hd.TrangThai == "Đã thanh toán";

            return View(vm);
        }

        // ==============================
        // SAVE HÓA ĐƠN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(InvoiceEditVM vm)
        {
            var hd = await _db.HoaDons.Include(h => h.DatBan).Include(h => h.ChiTiet)
                .FirstOrDefaultAsync(h => h.HoaDonID == vm.HoaDonID);

            if (hd == null) return NotFound();

            if (hd.TrangThai == "Đã thanh toán")
            {
                TempData["msg"] = "⚠️ Không thể sửa hóa đơn đã thanh toán.";
                return RedirectToAction(nameof(Edit), new { id = hd.HoaDonID });
            }

            // Lưu bàn
            var datBan = await _db.DatBans.FindAsync(hd.DatBanID);
            if (datBan != null)
            {
                datBan.BanPhongID = vm.BanPhongID;
            }

            hd.GiamGia = vm.GiamGia;
            hd.DiemSuDung = vm.DiemSuDung;
            hd.HinhThucThanhToan = vm.HinhThucThanhToan;

            await UpdateTongTienAsync(hd);
            await _db.SaveChangesAsync();

            TempData["msg"] = "💾 Đã lưu hóa đơn.";
            return RedirectToAction(nameof(Edit), new { id = hd.HoaDonID });
        }

        // ==============================
        // THÊM MÓN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int hoaDonId, int monAnId, int soLuong)
        {
            if (soLuong <= 0) soLuong = 1;

            var hd = await _db.HoaDons.Include(h => h.ChiTiet)
                .FirstOrDefaultAsync(h => h.HoaDonID == hoaDonId);

            if (hd == null) return NotFound();

            if (hd.TrangThai == "Đã thanh toán")
            {
                TempData["msg"] = "⚠️ Không thể thêm món vào hóa đơn đã thanh toán.";
                return RedirectToAction(nameof(Edit), new { id = hoaDonId });
            }

            var mon = await _db.MonAns.FindAsync(monAnId);
            if (mon == null) return NotFound();

            var ct = hd.ChiTiet.FirstOrDefault(x => x.MonAnID == monAnId);
            if (ct != null)
            {
                ct.SoLuong += soLuong;
                ct.ThanhTien = ct.SoLuong * ct.DonGia;
            }
            else
            {
                _db.ChiTietHoaDons.Add(new ChiTietHoaDon
                {
                    HoaDonID = hoaDonId,
                    MonAnID = monAnId,
                    SoLuong = soLuong,
                    DonGia = mon.DonGia,
                    ThanhTien = soLuong * mon.DonGia
                });
            }

            await UpdateTongTienAsync(hd);
            await _db.SaveChangesAsync();

            TempData["msg"] = "🍽 Đã thêm món vào hóa đơn.";
            return RedirectToAction(nameof(Edit), new { id = hoaDonId });
        }

        // ==============================
        // XÓA MÓN / GIẢM SỐ LƯỢNG
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int hoaDonId, int monAnId, bool removeAll = false)
        {
            var hd = await _db.HoaDons.Include(h => h.ChiTiet)
                .FirstOrDefaultAsync(h => h.HoaDonID == hoaDonId);

            if (hd == null) return NotFound();

            var ct = hd.ChiTiet.FirstOrDefault(x => x.MonAnID == monAnId);
            if (ct == null)
                return RedirectToAction(nameof(Edit), new { id = hoaDonId });

            if (removeAll || ct.SoLuong <= 1)
                _db.ChiTietHoaDons.Remove(ct);
            else
            {
                ct.SoLuong -= 1;
                ct.ThanhTien = ct.SoLuong * ct.DonGia;
            }

            await UpdateTongTienAsync(hd);
            await _db.SaveChangesAsync();

            TempData["msg"] = "🗑 Đã cập nhật món.";
            return RedirectToAction(nameof(Edit), new { id = hoaDonId });
        }

        // ==============================
        // UPDATE TONG TIEN
        // ==============================
        private Task UpdateTongTienAsync(HoaDon hd)
        {
            var sub = hd.ChiTiet.Sum(x => x.ThanhTien);
            var vat = sub * 0.1m;
            var final = sub + vat - hd.GiamGia - hd.DiemSuDung;
            if (final < 0) final = 0;
            hd.TongTien = final;

            return Task.CompletedTask;
        }

        // ==============================
        // DETAILS
        // ==============================
        public async Task<IActionResult> Details(int id)
        {
            var hd = await _db.HoaDons
                .Include(h => h.DatBan)
                .Include(h => h.ChiTiet).ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();
            return View(hd);
        }

        // ==============================
        // PRINT
        // ==============================
        public async Task<IActionResult> Print(int id)
        {
            var hd = await _db.HoaDons
                .Include(h => h.DatBan)
                .Include(h => h.ChiTiet).ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();
            return View(hd);
        }

        // ==============================
        // DELETE
        // ==============================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hd = await _db.HoaDons.Include(h => h.DatBan)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();
            return View(hd);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hd = await _db.HoaDons.FindAsync(id);
            if (hd != null)
            {
                _db.HoaDons.Remove(hd);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
