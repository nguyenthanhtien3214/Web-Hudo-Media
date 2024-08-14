using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using website.Data;
using website.Models;

namespace website.Controllers
{
    public class ThuTucChoThue : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThuTucChoThue(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var document = _context.Documents.FirstOrDefault();
            if (document == null)
            {
                return View();
            }

            // Đảm bảo rằng tài liệu là một file PDF
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", document.FileType);
            if (System.IO.File.Exists(filePath) && Path.GetExtension(filePath).ToLower() == ".pdf")
            {
                ViewData["DocumentPath"] = $"/uploads/{document.FileType}";
            }
            else
            {
                ViewData["DocumentPath"] = null;
            }

            return View(document);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var document = _context.Documents.FirstOrDefault(d => d.DocumentId == id);

            if (document == null)
            {
                // Tạo một đối tượng Document mới để hiển thị form chỉnh sửa
                document = new website.Models.Document();
            }

            return View(document);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(website.Models.Document document, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    document.FileType = file.FileName;
                }
                else if (document.DocumentId == 0) // Thêm điều kiện kiểm tra nếu là tài liệu mới
                {
                    // Nếu là tài liệu mới và không có tệp được tải lên, trả về lỗi hoặc thông báo cho người dùng
                    ModelState.AddModelError("", "Bạn phải tải lên một tệp.");
                    return View(document);
                }

                if (document.DocumentId == 0)
                {
                    _context.Documents.Add(document);
                }
                else
                {
                    _context.Documents.Update(document);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(document);
        }

        [Authorize(Roles = "Admin")]
        // Phương thức hiển thị danh sách các tài liệu
        public async Task<IActionResult> List(string message = null)
        {
            ViewData["Message"] = message; // Truyền thông báo tới view nếu có
            var documents = await _context.Documents.ToListAsync(); // Lấy tất cả tài liệu từ cơ sở dữ liệu
            return View(documents);
        }

        // Phương thức xử lý xóa tài liệu
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            _context.Documents.Remove(document); // Xóa tài liệu khỏi cơ sở dữ liệu
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            return RedirectToAction("List", new { message = "Xóa tài liệu thành công." }); // Chuyển hướng về trang danh sách tài liệu với thông báo
        }
    }
}
