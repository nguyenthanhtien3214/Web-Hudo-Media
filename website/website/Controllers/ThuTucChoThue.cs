using Microsoft.AspNetCore.Mvc;
using System.IO;
using Aspose.Words;
using Aspose.Words.Saving;
using website.Data;
using website.Models; // Đảm bảo bạn có namespace của Models
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", document.FileType);
            if (System.IO.File.Exists(filePath))
            {
                var htmlContent = GetHtmlContent(filePath);
                ViewData["DocumentContent"] = htmlContent;
            }

            return View(document);
        }

        public IActionResult Edit(int id)
        {
            var document = _context.Documents.FirstOrDefault(d => d.DocumentId == id);

            if (document == null)
            {
                // Tạo một đối tượng Document mới để hiển thị form chỉnh sửa
                document = new website.Models.Document(); // Sử dụng lớp Document từ website.Models
            }

            return View(document);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(website.Models.Document document, IFormFile file) // Sử dụng lớp Document từ website.Models
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

        private string GetHtmlContent(string filePath)
        {
            var doc = new Aspose.Words.Document(filePath); // Sử dụng lớp Document từ Aspose.Words
            using (var stream = new MemoryStream())
            {
                var htmlSaveOptions = new HtmlSaveOptions
                {
                    ExportImagesAsBase64 = true // Nhúng hình ảnh dưới dạng base64
                };

                // Gọi phương thức Save với các tham số chính xác
                doc.Save(stream, htmlSaveOptions);
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
