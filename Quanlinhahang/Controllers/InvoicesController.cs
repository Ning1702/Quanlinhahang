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
        public async Task<IActionResult> Index([FromQuery] InvoiceFilterVM f, [FromQuery] int status = 0)
        {
            ViewBag.Status = status;
            ViewBag.Filter = f;

            // 1. Bắt đầu truy vấn, Include tất cả các bảng liên quan
            var query = _db.HoaDons
                .Include(h => h.TrangThai)
                .Include(h => h.DatBan)
                    .ThenInclude(db => db.KhachHang) // Lấy KhachHang từ DatBan
                .Include(h => h.BanPhong)
                    .ThenInclude(bp => bp.LoaiBanPhong) // Lấy LoaiBanPhong từ BanPhong
                .Include(h => h.ChiTietHoaDons) // QUAN TRỌNG: Include CTHD để tính tổng
                .AsQueryable();

            // 2. Áp dụng các bộ lọc (Filter)
            if (status > 0)
            {
                query = query.Where(h => h.TrangThaiID == status);
            }

            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                string s = f.Search.Trim().ToLower();
                // Lọc theo KhachHang (từ DatBan)
                query = query.Where(h =>
                    (h.DatBan.KhachHang.HoTen.ToLower().Contains(s)) ||
                    (h.DatBan.KhachHang.SoDienThoai ?? "").Contains(s));
            }

            if (f.From.HasValue)
            {
                query = query.Where(h => h.NgayLap.Date >= f.From.Value.Date);
            }

            if (f.To.HasValue)
            {
                query = query.Where(h => h.NgayLap.Date <= f.To.Value.Date);
            }

            // 3. Chiếu (Select) sang ViewModel SAU KHI LỌC
            // Sử dụng một bước trung gian để tính SubTotal (Tạm tính)
            var projectedData = query.Select(h => new
            {
                HoaDon = h,
                SubTotal = h.ChiTietHoaDons.Sum(ct => ct.ThanhTien)
            })
            .Select(x => new InvoiceRowVM
            {
                HoaDonID = x.HoaDon.HoaDonID,
                NgayLap = x.HoaDon.NgayLap,
                KhachHang = x.HoaDon.DatBan.KhachHang.HoTen,
                SoDienThoai = x.HoaDon.DatBan.KhachHang.SoDienThoai,

                // Xử lý trường hợp BanPhongID là NULL
                BanPhong = x.HoaDon.BanPhong != null ? x.HoaDon.BanPhong.TenBanPhong : "Không yêu cầu",
                LoaiBanPhong = (x.HoaDon.BanPhong != null && x.HoaDon.BanPhong.LoaiBanPhong != null)
                               ? x.HoaDon.BanPhong.LoaiBanPhong.TenLoai : "",

                // TÍNH TOÁN THÀNH TIỀN CUỐI CÙNG (Giống trang Edit)
                ThanhTien = (x.SubTotal * (1 + (x.HoaDon.VAT ?? 0.10m)))
                            - x.HoaDon.GiamGia
                            - x.HoaDon.DiemSuDung,

                TrangThaiID = x.HoaDon.TrangThaiID,
                TrangThaiTen = x.HoaDon.TrangThai.TenTrangThai
            });

            // 4. Lấy dữ liệu và trả về View
            var list = await projectedData
                .OrderByDescending(x => x.NgayLap)
                .Take(500)
                .ToListAsync();

            return View(list);
        }

        // ==============================
        // CHUYỂN SANG "ĐANG PHỤC VỤ"
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartServing(int id, int status)
        {
            var hd = await _db.HoaDons.Include(h => h.TrangThai).FirstOrDefaultAsync(h => h.HoaDonID == id);
            if (hd == null) return NotFound();

            if (hd.TrangThaiID == 2) // Đã xác nhận
            {
                hd.TrangThaiID = 3; // Đang phục vụ
                await _db.SaveChangesAsync();
                TempData["msg"] = "✅ Đã chuyển sang trạng thái phục vụ.";
            }
            else
            {
                TempData["msg"] = "⚠️ Không thể phục vụ hóa đơn này.";
            }

            return RedirectToAction(nameof(Index), new { status });
        }

        // ==============================
        // XÁC NHẬN HÓA ĐƠN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmInvoice(int id, int status)
        {
            var hd = await _db.HoaDons.Include(h => h.TrangThai).FirstOrDefaultAsync(h => h.HoaDonID == id);
            if (hd == null) return NotFound();

            if (hd.TrangThaiID == 1) // Chờ xác nhận
            {
                hd.TrangThaiID = 2; // Đã xác nhận
                await _db.SaveChangesAsync();
                TempData["msg"] = "✅ Hóa đơn đã được xác nhận.";
            }
            else
            {
                TempData["msg"] = "⚠️ Không thể xác nhận hóa đơn này.";
            }

            return RedirectToAction(nameof(Index), new { status });
        }

        // ==============================
        // HỦY HÓA ĐƠN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyHoaDon(int id, int status)
        {
            var hd = await _db.HoaDons.Include(h => h.TrangThai).FirstOrDefaultAsync(h => h.HoaDonID == id);
            if (hd == null) return NotFound();

            if (hd.TrangThaiID != 4) // != Đã thanh toán
            {
                hd.TrangThaiID = 5; // Đã hủy
                await _db.SaveChangesAsync();
                TempData["msg"] = "🗑 Hóa đơn đã bị hủy.";
            }
            else
            {
                TempData["msg"] = "⚠️ Không thể hủy hóa đơn đã thanh toán.";
            }

            return RedirectToAction(nameof(Index), new { status });
        }

        // ==============================
        // THANH TOÁN HÓA ĐƠN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(int id, int status)
        {
            var hd = await _db.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .Include(h => h.TrangThai)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();

            if (hd.TrangThaiID == 3) // Đang phục vụ
            {
                await UpdateTongTienAsync(hd);
                hd.TrangThaiID = 4; // Đã thanh toán
                await _db.SaveChangesAsync();
                TempData["msg"] = "💰 Đã thanh toán.";
            }
            else
            {
                TempData["msg"] = "⚠️ Chỉ thanh toán hóa đơn đang phục vụ.";
            }

            return RedirectToAction(nameof(Index), new { status });
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
                TrangThaiID = 1 // Chờ xác nhận
            };

            _db.HoaDons.Add(hd);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = hd.HoaDonID });
        }

        // ==============================
        // EDIT
        // ==============================
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.ReturnUrl = Request.Headers["Referer"].ToString();

            var hd = await _db.HoaDons
                .Include(h => h.ChiTietHoaDons).ThenInclude(ct => ct.MonAn)
                .Include(h => h.DatBan)
                .Include(h => h.TrangThai)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();

            var vm = new InvoiceEditVM
            {
                HoaDonID = hd.HoaDonID,
                DatBanID = hd.DatBanID,
                BanPhongID = hd.BanPhongID,
                GiamGia = hd.GiamGia,
                DiemSuDung = hd.DiemSuDung,
                HinhThucThanhToan = hd.HinhThucThanhToan,
                TrangThai = hd.TrangThai.TenTrangThai,

                Items = hd.ChiTietHoaDons.Select(ct => new InvoiceEditVM.ItemLine
                {
                    MonAnID = ct.MonAnID,
                    TenMon = ct.MonAn.TenMon,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia
                }).ToList()
            };

            ViewBag.MonAn = await _db.MonAns.Where(m => m.TrangThai == "Còn bán").ToListAsync();
            ViewBag.BanPhongs = await _db.BanPhongs.ToListAsync();
            ViewBag.TrangThai = hd.TrangThai.TenTrangThai;
            ViewBag.DaThanhToan = (hd.TrangThaiID == 4);

            return View(vm);
        }

        // =======================================================
        // ===== CHọn bàn =====
        // =======================================================
        [HttpGet]
        public async Task<IActionResult> GetBanPhongStatus()
        {
            // Lấy tất cả bàn, sắp xếp theo Loại, rồi đến Tên
            var banPhongs = await _db.BanPhongs
                                .Include(b => b.LoaiBanPhong)
                                .OrderBy(b => b.LoaiBanPhong.LoaiBanPhongID)
                                .ThenBy(b => b.BanPhongID)
                                .ToListAsync();

            // Trả về dữ liệu dạng JSON cho AJAX
            return Json(banPhongs);
        }

        // ==============================
        // SAVE
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(InvoiceEditVM vm, string returnUrl)
        {
            var hd = await _db.HoaDons
                .Include(h => h.DatBan)
                .Include(h => h.ChiTietHoaDons)
                .FirstOrDefaultAsync(h => h.HoaDonID == vm.HoaDonID);

            if (hd == null) return NotFound();

            hd.BanPhongID = vm.BanPhongID;


            hd.GiamGia = vm.GiamGia;
            hd.DiemSuDung = vm.DiemSuDung;
            hd.HinhThucThanhToan = vm.HinhThucThanhToan;

            await UpdateTongTienAsync(hd);
            await _db.SaveChangesAsync();

            TempData["msg"] = "Đã lưu hóa đơn.";

            return RedirectToAction(nameof(Edit), new { id = vm.HoaDonID });
        }

        // ==============================
        // ADD ITEM
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int hoaDonId, int monAnId, int soLuong)
        {
            if (soLuong <= 0) soLuong = 1;

            var hd = await _db.HoaDons.Include(h => h.ChiTietHoaDons).FirstOrDefaultAsync(h => h.HoaDonID == hoaDonId);
            if (hd == null) return NotFound();

            if (hd.TrangThaiID == 4)
            {
                TempData["msg"] = "⚠️ Không thể thêm món vào hóa đơn đã thanh toán.";
                return RedirectToAction(nameof(Edit), new { id = hoaDonId });
            }

            var mon = await _db.MonAns.FindAsync(monAnId);
            if (mon == null) return NotFound();

            var ct = hd.ChiTietHoaDons.FirstOrDefault(x => x.MonAnID == monAnId);

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

            TempData["msg"] = "🍽 Đã thêm món.";
            return RedirectToAction(nameof(Edit), new { id = hoaDonId });
        }

        // ==============================
        // REMOVE ITEM
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int hoaDonId, int monAnId, bool removeAll = false)
        {
            var hd = await _db.HoaDons.Include(h => h.ChiTietHoaDons).FirstOrDefaultAsync(h => h.HoaDonID == hoaDonId);
            if (hd == null) return NotFound();

            var ct = hd.ChiTietHoaDons.FirstOrDefault(x => x.MonAnID == monAnId);
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
        // UPDATE TỔNG TIỀN
        // ==============================
        private Task UpdateTongTienAsync(HoaDon hd)
        {
            var sub = hd.ChiTietHoaDons.Sum(x => x.ThanhTien);
            var vat = sub * 0.1m;
            var final = sub + vat - hd.GiamGia - hd.DiemSuDung;
            if (final < 0) final = 0;

            hd.TongTien = final;
            return Task.CompletedTask;
        }

        // ==============================
        // DETAILS
        // ==============================
        public async Task<IActionResult> Details(int id, int status = 0)

        {
            var hd = await _db.HoaDons
                .Include(h => h.DatBan).ThenInclude(db => db.BanPhong).ThenInclude(bp => bp.LoaiBanPhong)
                .Include(h => h.ChiTietHoaDons).ThenInclude(ct => ct.MonAn)
                .Include(h => h.TrangThai)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();

            ViewBag.Status = status;

            return View(hd);
        }

        // ==============================
        // PRINT
        // ==============================
        public async Task<IActionResult> Print(int id)
        {
            var hd = await _db.HoaDons
                .Include(h => h.DatBan)
                .Include(h => h.ChiTietHoaDons).ThenInclude(ct => ct.MonAn)
                .Include(h => h.TrangThai)
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

            var hd = await _db.HoaDons
                .Include(h => h.DatBan)
                .Include(h => h.TrangThai)
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

        public IActionResult GoManage(int id)
        {
            var hd = _db.HoaDons.FirstOrDefault(x => x.HoaDonID == id);
            if (hd == null) return NotFound();

            // Điều hướng sang tab đúng theo trạng thái
            return RedirectToAction("Index", new
            {
                status = hd.TrangThaiID,
                highlight = id // gửi ID để highlight dòng đó
            });
        }

    }
}
