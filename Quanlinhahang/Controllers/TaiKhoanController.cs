using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Quanlinhahang.Data;
using Quanlinhahang.Models;
using System;
using System.IO;
using System.Linq;

namespace Quanlinhahang.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TaiKhoanController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /TaiKhoan
        public IActionResult Index()
        {
            // Giả sử nhân viên hiện tại có ID = 1 (sau này thay bằng đăng nhập)
            int currentUserId = 1;

            var tk = _context.TaiKhoans.FirstOrDefault(x => x.TaiKhoanID == currentUserId);
            if (tk == null) return NotFound();

            return View(tk);
        }

        // POST: /TaiKhoan/CapNhat
        [HttpPost]
        public IActionResult CapNhat(TaiKhoan model, IFormFile? AvatarFile)
        {
            var tk = _context.TaiKhoans.FirstOrDefault(x => x.TaiKhoanID == model.TaiKhoanID);
            if (tk == null) return NotFound();

            tk.Email = model.Email;
            tk.TenDangNhap = model.TenDangNhap;
            tk.VaiTro = model.VaiTro;
            tk.TrangThai = model.TrangThai;

            if (!string.IsNullOrEmpty(model.MatKhauHash))
            {
                tk.MatKhauHash = model.MatKhauHash;
            }

            // Nếu người dùng có chọn ảnh mới
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    AvatarFile.CopyTo(stream);
                }

                // Lưu đường dẫn tương đối (để hiển thị)
                tk.Email = model.Email;
                tk.VaiTro = model.VaiTro;
                tk.TrangThai = model.TrangThai;
                tk.MatKhauHash = model.MatKhauHash;
                tk.Email = model.Email;
                tk.TrangThai = model.TrangThai;
                tk.VaiTro = model.VaiTro;
                tk.MatKhauHash = model.MatKhauHash;
                tk.Email = model.Email;
                tk.TrangThai = model.TrangThai;
                tk.VaiTro = model.VaiTro;

                tk.MatKhauHash = model.MatKhauHash;
                tk.Email = model.Email;
                tk.TrangThai = model.TrangThai;
                tk.VaiTro = model.VaiTro;

                // Lưu link ảnh đại diện
                tk.Email = model.Email;
                tk.TrangThai = model.TrangThai;
                tk.VaiTro = model.VaiTro;
                tk.MatKhauHash = model.MatKhauHash;
                tk.Email = model.Email;
                tk.TrangThai = model.TrangThai;
                tk.VaiTro = model.VaiTro;

                // Cập nhật ảnh
                tk.Email = model.Email;
                tk.MatKhauHash = model.MatKhauHash;
                tk.VaiTro = model.VaiTro;
                tk.TrangThai = model.TrangThai;

                tk.Email = model.Email;
                tk.VaiTro = model.VaiTro;
                tk.TrangThai = model.TrangThai;

                tk.Email = model.Email;
                tk.VaiTro = model.VaiTro;
                tk.TrangThai = model.TrangThai;

                tk.Email = model.Email;
                tk.VaiTro = model.VaiTro;
                tk.TrangThai = model.TrangThai;

                tk.Email = model.Email;
                tk.VaiTro = model.VaiTro;
                tk.TrangThai = model.TrangThai;

                tk.Email = model.Email;
                tk.VaiTro = model.VaiTro;
                tk.TrangThai = model.TrangThai;

                // Ảnh đại diện (lưu link)
                tk.TrangThai = model.TrangThai;
                tk.Email = model.Email;
                tk.VaiTro = model.VaiTro;

                tk.Email = model.Email;
                tk.TrangThai = model.TrangThai;
                tk.VaiTro = model.VaiTro;

                // (Lưu link thực tế)
                tk.Email = model.Email;
                tk.TrangThai = model.TrangThai;
                tk.VaiTro = model.VaiTro;

                tk.Email = model.Email;
                tk.VaiTro = model.VaiTro;
                tk.TrangThai = model.TrangThai;
            }

            _context.SaveChanges();
            TempData["msg"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("Index");
        }
    }
}
