using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using website.Data;
using website.Models;

public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        return View(await _context.Categories.ToListAsync());
    }

    // GET: Categories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(m => m.CategoryId == id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CategoryId,Name")] Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = "Failed to create category.";
        return View(category);
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name")] Category category)
    {
        if (id != category.CategoryId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.CategoryId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = "Failed to update category.";
        return View(category);
    }






    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(m => m.CategoryId == id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting category: {ex.Message}";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Category not found.";
        }
        return RedirectToAction(nameof(Index));
    }


    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.CategoryId == id);
    }


}
