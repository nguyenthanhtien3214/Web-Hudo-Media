using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using website.Data;
using website.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace website.Controllers
{
    public class ProductImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductImagesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        // Hiển thị danh sách ảnh để xóa
        [HttpGet]
        public async Task<IActionResult> DeleteImages()
        {
            var images = await _context.ProductImages.ToListAsync();
            return View(images);
        }
        [Authorize(Roles = "Admin")]
        // Phương thức xóa ảnh
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage == null)
            {
                return NotFound();
            }

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", productImage.ImageUrl);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();

            return RedirectToAction("DeleteImages");
        }
    }
}
