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
        int pageSize = 5; // Số lượng sản phẩm trên mỗi trang
        int pageNumber = (page ?? 1);

        var products = await _context.Products.Include(p => p.Category).ToListAsync();
        var pagedProducts = products.ToPagedList(pageNumber, pageSize);

        return View(pagedProducts);
    }

    public async Task<IActionResult> ListSanPham(int? page)
    {
        int pageSize = 5; // Số lượng sản phẩm trên mỗi trang
        int pageNumber = (page ?? 1);

        var products = await _context.Products.Include(p => p.Category).ToListAsync();
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
    public async Task<IActionResult> ListSanPham(ProductViewModel viewModel)
    {
        // Kiểm tra ModelState trước khi xóa ModelState
        if (ModelState.IsValid)
        {
            var resultMessage = new SqlParameter("@ResultMessage", System.Data.SqlDbType.NVarChar, 200)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC sp_AddProduct @Name, @Description, @Price, @Image, @Quantity, @CategoryId, @ResultMessage OUTPUT",
                new SqlParameter("@Name", viewModel.NewProduct.Name),
                new SqlParameter("@Description", viewModel.NewProduct.Description),
                new SqlParameter("@Price", viewModel.NewProduct.Price),
                new SqlParameter("@Image", viewModel.NewProduct.ImageUrl),
                new SqlParameter("@Quantity", viewModel.NewProduct.Quantity),
                new SqlParameter("@CategoryId", viewModel.NewProduct.CategoryId),
                resultMessage);

            ViewBag.ResultMessage = resultMessage.Value.ToString();
            if (resultMessage.Value.ToString() == "Thêm sản phẩm thành công")
            {
                return RedirectToAction(nameof(ListSanPham));
            }
        }
        else
        {
            var errors = ModelState.SelectMany(x => x.Value.Errors)
                                   .Select(x => x.ErrorMessage)
                                   .ToList();
            ViewBag.ModelStateErrors = errors;
        }

        int pageSize = 5; // Số lượng sản phẩm trên mỗi trang 
        int pageNumber = 1;

        var products = await _context.Products.Include(p => p.Category).ToListAsync();
        var pagedProducts = products.ToPagedList(pageNumber, pageSize);

        viewModel.Products = pagedProducts;
        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
        return View(viewModel);
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
        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.ProductId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var resultMessage = new SqlParameter("@ResultMessage", System.Data.SqlDbType.NVarChar, 200)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC sp_UpdateProduct @ProductId, @Name, @Description, @Price, @ImageUrl, @Quantity, @CategoryId, @ResultMessage OUTPUT",
                new SqlParameter("@ProductId", product.ProductId),
                new SqlParameter("@Name", product.Name),
                new SqlParameter("@Description", product.Description),
                new SqlParameter("@Price", product.Price),
                new SqlParameter("@ImageUrl", product.ImageUrl),
                new SqlParameter("@Quantity", product.Quantity),
                new SqlParameter("@CategoryId", product.CategoryId),
                resultMessage);

            ViewBag.ResultMessage = resultMessage.Value.ToString();
            if (resultMessage.Value.ToString() == "Sửa sản phẩm thành công")
            {
                return RedirectToAction(nameof(Index));
            }
        }
        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
        return View(product);
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
        var resultMessage = new SqlParameter("@ResultMessage", System.Data.SqlDbType.NVarChar, 200)
        {
            Direction = System.Data.ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync("EXEC sp_DeleteProduct @ProductId, @ResultMessage OUTPUT",
            new SqlParameter("@ProductId", id),
            resultMessage);

        ViewBag.ResultMessage = resultMessage.Value.ToString();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.ProductId == id);
    }
}
