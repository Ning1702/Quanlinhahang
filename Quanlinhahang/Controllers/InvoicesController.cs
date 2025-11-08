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
                           TrangThaiThanhToan = h.TrangThaiThanhToan,
                           TrangThaiXacNhan = h.TrangThaiXacNhan,
                           LoaiDatBan = d.LoaiDatBan
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

        // ✅ XÁC NHẬN: chuyển sang "Đã xác nhận"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmInvoice(int id)
        {
            var hd = await _db.HoaDons.FindAsync(id);
            if (hd == null) return NotFound();

            if (hd.TrangThaiThanhToan == "Đã thanh toán")
            {
                TempData["msg"] = "⚠️ Hóa đơn đã thanh toán, không thể xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            if (hd.TrangThaiXacNhan == null
                || hd.TrangThaiXacNhan == "Chờ xác nhận"
                || hd.TrangThaiXacNhan == "Chưa xác nhận")
            {
                hd.TrangThaiXacNhan = "Đã xác nhận";
                await _db.SaveChangesAsync();
                TempData["msg"] = "✅ Hóa đơn đã được xác nhận.";
            }
            else if (hd.TrangThaiXacNhan == "Đã xác nhận")
            {
                TempData["msg"] = "⚠️ Hóa đơn này đã được xác nhận rồi.";
            }
            else if (hd.TrangThaiXacNhan == "Đã hủy")
            {
                TempData["msg"] = "⚠️ Hóa đơn đã bị hủy, không thể xác nhận lại.";
            }
            else
            {
                TempData["msg"] = "⚠️ Trạng thái hóa đơn không phù hợp để xác nhận.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ❌ HỦY HÓA ĐƠN (chỉ khi chưa thanh toán)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyHoaDon(int id)
        {
            var hd = await _db.HoaDons.FindAsync(id);
            if (hd == null) return NotFound();

            if (hd.TrangThaiThanhToan == "Đã thanh toán")
            {
                TempData["msg"] = "⚠️ Hóa đơn đã thanh toán, không thể hủy.";
                return RedirectToAction(nameof(Index));
            }

            // Cho hủy khi đang ở trạng thái đã xác nhận hoặc chờ xác nhận
            if (hd.TrangThaiXacNhan == "Đã xác nhận"
                || hd.TrangThaiXacNhan == "Chờ xác nhận"
                || hd.TrangThaiXacNhan == "Chưa xác nhận")
            {
                hd.TrangThaiXacNhan = "Đã hủy";
                await _db.SaveChangesAsync();
                TempData["msg"] = "✅ Hóa đơn đã được hủy.";
            }
            else
            {
                TempData["msg"] = "⚠️ Trạng thái hóa đơn không phù hợp để hủy.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ✅ THANH TOÁN: chuyển sang "Đã thanh toán"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(int id)
        {
            var hd = await _db.HoaDons.FindAsync(id);
            if (hd == null) return NotFound();

            if (hd.TrangThaiThanhToan == "Đã thanh toán")
            {
                TempData["msg"] = "⚠️ Hóa đơn này đã thanh toán rồi.";
            }
            else
            {
                hd.TrangThaiThanhToan = "Đã thanh toán";
                await _db.SaveChangesAsync();
                TempData["msg"] = "✅ Hóa đơn đã được thanh toán.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Invoices/Create
        public async Task<IActionResult> Create(int? datBanId)
        {
            if (datBanId == null)
                return BadRequest("Thiếu DatBanID");

            var datBan = await _db.DatBans.FirstOrDefaultAsync(x => x.DatBanID == datBanId);
            if (datBan == null) return NotFound("Không tìm thấy lịch đặt.");

            var hd = new HoaDon
            {
                DatBanID = datBan.DatBanID,
                NgayLap = DateTime.Now,
                TongTien = 0,
                GiamGia = 0,
                DiemCong = 0,
                DiemSuDung = 0,
                TrangThaiThanhToan = "Chưa thanh toán",
                TrangThaiXacNhan = "Chờ xác nhận"   // lúc mới tạo: chờ xác nhận
            };
            _db.HoaDons.Add(hd);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = hd.HoaDonID });
        }

        // GET: /Invoices/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var hd = await _db.HoaDons
                .Include(h => h.ChiTiet).ThenInclude(ct => ct.MonAn)
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

            ViewBag.TrangThaiXacNhan = hd.TrangThaiXacNhan ?? "Chờ xác nhận";
            ViewBag.DaThanhToan = hd.TrangThaiThanhToan == "Đã thanh toán";

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(InvoiceEditVM vm)
        {
            var hd = await _db.HoaDons.FindAsync(vm.HoaDonID);
            if (hd == null) return NotFound();

            if (hd.TrangThaiThanhToan == "Đã thanh toán")
            {
                TempData["msg"] = "⚠️ Hóa đơn đã thanh toán, không thể chỉnh sửa.";
                return RedirectToAction(nameof(Edit), new { id = hd.HoaDonID });
            }

            hd.GiamGia = vm.GiamGia;
            hd.DiemSuDung = vm.DiemSuDung;
            hd.HinhThucThanhToan = vm.HinhThucThanhToan;
            hd.TrangThaiThanhToan = vm.TrangThaiThanhToan;

            await UpdateTongTienAsync(hd.HoaDonID);
            await _db.SaveChangesAsync();

            TempData["msg"] = "💾 Đã lưu hóa đơn.";
            return RedirectToAction(nameof(Edit), new { id = hd.HoaDonID });
        }

        // ✅ THÊM MÓN VÀO HÓA ĐƠN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int hoaDonId, int monAnId, int soLuong)
        {
            if (soLuong <= 0) soLuong = 1;

            var hd = await _db.HoaDons
                .Include(h => h.ChiTiet)
                .FirstOrDefaultAsync(h => h.HoaDonID == hoaDonId);

            if (hd == null) return NotFound();

            if (hd.TrangThaiThanhToan == "Đã thanh toán")
            {
                TempData["msg"] = "⚠️ Hóa đơn đã thanh toán, không thể thêm món.";
                return RedirectToAction(nameof(Edit), new { id = hoaDonId });
            }

            var mon = await _db.MonAns.FindAsync(monAnId);
            if (mon == null)
            {
                TempData["msg"] = "⚠️ Không tìm thấy món ăn.";
                return RedirectToAction(nameof(Edit), new { id = hoaDonId });
            }

            // Nếu món đã tồn tại trong hóa đơn thì cộng số lượng
            var ct = hd.ChiTiet.FirstOrDefault(x => x.MonAnID == monAnId);
            if (ct != null)
            {
                ct.SoLuong += soLuong;
                ct.ThanhTien = ct.SoLuong * ct.DonGia;
                _db.ChiTietHoaDons.Update(ct);
            }
            else
            {
                var newCt = new ChiTietHoaDon
                {
                    HoaDonID = hoaDonId,
                    MonAnID = monAnId,
                    SoLuong = soLuong,
                    DonGia = mon.DonGia,
                    ThanhTien = mon.DonGia * soLuong
                };
                _db.ChiTietHoaDons.Add(newCt);
            }

            await _db.SaveChangesAsync();
            await UpdateTongTienAsync(hoaDonId);

            TempData["msg"] = "✅ Đã thêm món vào hóa đơn.";
            return RedirectToAction(nameof(Edit), new { id = hoaDonId });
        }

        // ❌ GIẢM SỐ LƯỢNG / XÓA MÓN KHỎI HÓA ĐƠN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int hoaDonId, int monAnId, bool removeAll = false)
        {
            var hd = await _db.HoaDons.FindAsync(hoaDonId);
            if (hd == null) return NotFound();

            if (hd.TrangThaiThanhToan == "Đã thanh toán")
            {
                TempData["msg"] = "⚠️ Hóa đơn đã thanh toán, không thể chỉnh sửa món.";
                return RedirectToAction(nameof(Edit), new { id = hoaDonId });
            }

            var ct = await _db.ChiTietHoaDons
                .FirstOrDefaultAsync(x => x.HoaDonID == hoaDonId && x.MonAnID == monAnId);

            if (ct != null)
            {
                if (removeAll || ct.SoLuong <= 1)
                {
                    // Xóa hẳn món khỏi hóa đơn
                    _db.ChiTietHoaDons.Remove(ct);
                    TempData["msg"] = "✅ Đã xóa món khỏi hóa đơn.";
                }
                else
                {
                    // Giảm bớt 1 đơn vị (SL luôn ≥ 1)
                    ct.SoLuong -= 1;
                    ct.ThanhTien = ct.SoLuong * ct.DonGia;
                    _db.ChiTietHoaDons.Update(ct);
                    TempData["msg"] = "✅ Đã giảm bớt 1 phần món trong hóa đơn.";
                }

                await _db.SaveChangesAsync();
                await UpdateTongTienAsync(hoaDonId);
            }

            return RedirectToAction(nameof(Edit), new { id = hoaDonId });
        }

        private async Task UpdateTongTienAsync(int hoaDonId)
        {
            var hd = await _db.HoaDons
                .Include(h => h.ChiTiet)
                .FirstOrDefaultAsync(h => h.HoaDonID == hoaDonId);
            if (hd == null) return;

            var sum = hd.ChiTiet.Sum(x => x.ThanhTien);
            sum -= hd.GiamGia;
            if (sum < 0) sum = 0;
            hd.TongTien = sum;

            _db.HoaDons.Update(hd);
            await _db.SaveChangesAsync();
        }

        // GET: /Invoices/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var hd = await _db.HoaDons
                .Include(h => h.DatBan)
                .Include(h => h.ChiTiet).ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();

            return View(hd);   // Views/Invoices/Details.cshtml (model: HoaDon)
        }

        // GET: /Invoices/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var hd = await _db.HoaDons
                .Include(h => h.DatBan)
                .Include(h => h.ChiTiet).ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(h => h.HoaDonID == id);

            if (hd == null) return NotFound();

            return View(hd);   // Views/Invoices/Print.cshtml (model: HoaDon)
        }
    }
}
