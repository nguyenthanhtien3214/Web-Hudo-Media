using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using website.Data;
using website.Models;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(int? page)
    {
        int pageSize = 5;
        int pageNumber = (page ?? 1);

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images) // Include images
            .ToListAsync();

        var pagedProducts = products.ToPagedList(pageNumber, pageSize);
        return View(pagedProducts);
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListSanPham(int? page)
    {
        int pageSize = 5;
        int pageNumber = (page ?? 1);

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images) // Include images
            .ToListAsync();

        var pagedProducts = products.ToPagedList(pageNumber, pageSize);

        var viewModel = new ProductViewModel
        {
            Products = pagedProducts,
            NewProduct = new Product()
        };

        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
        return View(viewModel);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> ListSanPham(ProductViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == viewModel.NewProduct.Name);

                if (existingProduct != null)
                {
                    ViewBag.ResultMessage = "Tên sản phẩm đã tồn tại. Vui lòng chọn tên khác.";
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
                    viewModel.Products = await GetPagedProductsAsync(1, 5);
                    return View(viewModel);
                }

                if (string.IsNullOrWhiteSpace(viewModel.NewProduct.Name) ||
                    string.IsNullOrWhiteSpace(viewModel.NewProduct.Description) ||
                    viewModel.NewProduct.Price <= 0 ||
                    viewModel.NewProduct.Quantity <= 0 ||
                    viewModel.NewProduct.CategoryId == 0 ||
                    viewModel.Files == null || !viewModel.Files.Any())
                {
                    ViewBag.ResultMessage = "Vui lòng điền đầy đủ các trường bắt buộc và chọn tệp hình ảnh.";
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
                    viewModel.Products = await GetPagedProductsAsync(1, 5);
                    return View(viewModel);
                }

                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                foreach (var file in viewModel.Files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    viewModel.NewProduct.Images.Add(new ProductImage { ImageUrl = "images/" + fileName });
                }

                _context.Add(viewModel.NewProduct);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm sản phẩm thành công";
                return RedirectToAction(nameof(ListSanPham));
            }
            catch (Exception ex)
            {
                ViewBag.ResultMessage = $"Error: {ex.Message}";
            }
        }
        else
        {
            var errors = ModelState.SelectMany(x => x.Value.Errors)
                                   .Select(x => x.ErrorMessage)
                                   .ToList();
            ViewBag.ModelStateErrors = errors;
        }

        viewModel.Products = await GetPagedProductsAsync(1, 5);
        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
        return View(viewModel);
    }
    [Authorize(Roles = "Admin")]
    private async Task<IPagedList<Product>> GetPagedProductsAsync(int pageNumber, int pageSize)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images) // Include images
            .ToListAsync();

        return products.ToPagedList(pageNumber, pageSize);
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
            return RedirectToAction("ListSanPham");
        }

        // Hiển thị giao diện xác nhận xóa, nếu cần
        return View(product);  // Hoặc trả về một View khác để xác nhận
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product != null)
            {
                // Xóa sản phẩm khỏi cơ sở dữ liệu
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa sản phẩm thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
        }

        return RedirectToAction(nameof(ListSanPham));
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound();
        }

        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
        return View(product);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> files)
    {
        if (id != product.ProductId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingProduct = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Quantity = product.Quantity;
                existingProduct.CategoryId = product.CategoryId;

                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                if (files != null && files.Count > 0)
                {
                    // Xóa các ảnh cũ
                    foreach (var image in existingProduct.Images)
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    existingProduct.Images.Clear();

                    // Thêm các ảnh mới
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadsDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        existingProduct.Images.Add(new ProductImage { ImageUrl = "images/" + fileName });
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Sửa sản phẩm thành công";
                return RedirectToAction(nameof(ListSanPham));
            }
            catch (Exception ex)
            {
                ViewBag.ResultMessage = $"Error: {ex.Message}";
            }
        }

        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
        return View(product);
    }



}
