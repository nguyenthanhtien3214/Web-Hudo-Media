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
using X.PagedList.Mvc.Core;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? page)
    {
        int pageSize = 5;
        int pageNumber = (page ?? 1);

        var products = await _context.Products
            .Include(p => p.Category)
            .Select(p => new Product
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description ?? string.Empty, // Xử lý null cho Description
                Price = p.Price,
                ImageUrl = p.ImageUrl ?? string.Empty, // Xử lý null cho ImageUrl
                Quantity = p.Quantity,
                CategoryId = p.CategoryId,
                Category = new Category
                {
                    CategoryId = p.Category.CategoryId,
                    Name = p.Category.Name ?? string.Empty // Xử lý null cho Category Name
                }
            })
            .ToListAsync();

        var pagedProducts = products.ToPagedList(pageNumber, pageSize);
        return View(pagedProducts);
    }

    public async Task<IActionResult> ListSanPham(int? page)
    {
        int pageSize = 5;
        int pageNumber = (page ?? 1);

        var products = await _context.Products
            .Include(p => p.Category)
            .Select(p => new Product
            {
                ProductId = p.ProductId,
                Name = p.Name ?? string.Empty, // Xử lý null cho Name
                Description = p.Description ?? string.Empty, // Xử lý null cho Description
                Price = p.Price,
                Image = p.Image ?? string.Empty, // Xử lý null cho Image
                ImageUrl = p.ImageUrl ?? string.Empty, // Xử lý null cho ImageUrl
                Quantity = p.Quantity,
                CategoryId = p.CategoryId,
                Category = new Category
                {
                    CategoryId = p.Category.CategoryId,
                    Name = p.Category.Name ?? string.Empty // Xử lý null cho Category Name
                }
            })
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

    [HttpPost]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> ListSanPham(ProductViewModel viewModel, IFormFile file)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Kiểm tra xem tên sản phẩm có bị trùng không
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == viewModel.NewProduct.Name);

                if (existingProduct != null)
                {
                    ViewBag.ResultMessage = "Tên sản phẩm đã tồn tại. Vui lòng chọn tên khác.";
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
                    viewModel.Products = await GetPagedProductsAsync(1, 5);
                    return View(viewModel);
                }

                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(viewModel.NewProduct.Name) ||
                    string.IsNullOrWhiteSpace(viewModel.NewProduct.Description) ||
                    viewModel.NewProduct.Price <= 0 ||
                    viewModel.NewProduct.Quantity <= 0 ||
                    viewModel.NewProduct.CategoryId == 0 ||
                    file == null || file.Length == 0)
                {
                    ViewBag.ResultMessage = "Vui lòng điền đầy đủ các trường bắt buộc và chọn tệp hình ảnh.";
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
                    viewModel.Products = await GetPagedProductsAsync(1, 5);
                    return View(viewModel);
                }

                // Xử lý việc upload file
                var fileName = Path.GetFileName(file.FileName);
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                viewModel.NewProduct.Image = fileName;
                viewModel.NewProduct.ImageUrl = "images/" + fileName;

                // Tạo câu lệnh SQL để thêm sản phẩm
                string sqlQuery = "INSERT INTO products (name, description, price, image, image_url, quantity, category_id) " +
                                  "VALUES (@Name, @Description, @Price, @Image, @ImageUrl, @Quantity, @CategoryId)";

                var parameters = new[]
                {
                new SqlParameter("@Name", viewModel.NewProduct.Name ?? (object)DBNull.Value),
                new SqlParameter("@Description", viewModel.NewProduct.Description ?? (object)DBNull.Value),
                new SqlParameter("@Price", viewModel.NewProduct.Price),
                new SqlParameter("@Image", viewModel.NewProduct.Image ?? (object)DBNull.Value),
                new SqlParameter("@ImageUrl", viewModel.NewProduct.ImageUrl ?? (object)DBNull.Value),
                new SqlParameter("@Quantity", viewModel.NewProduct.Quantity),
                new SqlParameter("@CategoryId", viewModel.NewProduct.CategoryId)
            };

                await _context.Database.ExecuteSqlRawAsync(sqlQuery, parameters);

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

    private async Task<IPagedList<Product>> GetPagedProductsAsync(int pageNumber, int pageSize)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Select(p => new Product
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description ?? string.Empty,
                Price = p.Price,
                Image = p.Image ?? string.Empty,
                ImageUrl = p.ImageUrl ?? string.Empty,
                Quantity = p.Quantity,
                CategoryId = p.CategoryId,
                Category = new Category
                {
                    CategoryId = p.Category.CategoryId,
                    Name = p.Category.Name ?? string.Empty
                }
            })
            .ToListAsync();

        return products.ToPagedList(pageNumber, pageSize);
    }



    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.ProductId == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // Sử dụng lệnh SQL trực tiếp để xóa sản phẩm
            var result = await _context.Database.ExecuteSqlRawAsync("DELETE FROM products WHERE product_id = @ProductId",
                new SqlParameter("@ProductId", id));

            if (result > 0)
            {
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToAction(nameof(ListSanPham));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Xử lý giá trị null cho các thuộc tính của sản phẩm
        product.Name = product.Name ?? string.Empty;
        product.Description = product.Description ?? string.Empty;
        product.Image = product.Image ?? string.Empty;
        product.ImageUrl = product.ImageUrl ?? string.Empty;

        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product, IFormFile file)
    {
        if (id != product.ProductId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Kiểm tra nếu file ảnh được chọn, nếu không giữ nguyên ảnh hiện tại
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    product.Image = fileName;
                    product.ImageUrl = "images/" + fileName;
                }
                else
                {
                    var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
                    if (existingProduct != null)
                    {
                        product.Image = existingProduct.Image;
                        product.ImageUrl = existingProduct.ImageUrl;
                    }
                }

                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(product.Name) ||
                    string.IsNullOrWhiteSpace(product.Description) ||
                    product.Price <= 0 ||
                    product.Quantity <= 0 ||
                    product.CategoryId == 0 ||
                    string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    ViewBag.ResultMessage = "Vui lòng điền đầy đủ các trường bắt buộc.";
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                    return View(product);
                }

                // Sử dụng lệnh SQL để cập nhật sản phẩm
                string sqlQuery = "UPDATE products SET name = @Name, description = @Description, price = @Price, " +
                                  "image = @Image, image_url = @ImageUrl, quantity = @Quantity, category_id = @CategoryId " +
                                  "WHERE product_id = @ProductId";

                var parameters = new[]
                {
                new SqlParameter("@Name", product.Name ?? (object)DBNull.Value),
                new SqlParameter("@Description", product.Description ?? (object)DBNull.Value),
                new SqlParameter("@Price", product.Price),
                new SqlParameter("@Image", product.Image ?? (object)DBNull.Value),
                new SqlParameter("@ImageUrl", product.ImageUrl ?? (object)DBNull.Value),
                new SqlParameter("@Quantity", product.Quantity),
                new SqlParameter("@CategoryId", product.CategoryId),
                new SqlParameter("@ProductId", product.ProductId)
            };

                await _context.Database.ExecuteSqlRawAsync(sqlQuery, parameters);

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
