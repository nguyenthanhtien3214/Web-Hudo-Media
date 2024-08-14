using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website.Data;
using website.Models;
using System.Threading.Tasks;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;

public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var customers = await _context.Customers
            .Select(c => new Customer
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName ?? string.Empty, // Nếu FullName là NULL, sử dụng chuỗi rỗng
                Email = c.Email ?? string.Empty, // Tương tự cho Email
                Phone = c.Phone ?? string.Empty, // Tương tự cho Phone
                Address = c.Address ?? string.Empty, // Tương tự cho Address
                Notes = c.Notes ?? string.Empty // Tương tự cho Notes
            })
            .ToListAsync();

        return View(customers);
    }

    [Authorize(Roles = "Admin")]
    // GET: Customers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }
        return View(customer);
    }
    [Authorize(Roles = "Admin")]
    // POST: Customers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,Email,Phone,Address,Notes")] Customer customer)
    {
        if (id != customer.CustomerId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Customer updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(customer.CustomerId))
                {
                    TempData["ErrorMessage"] = "Customer not found.";
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(customer);
    }
    [Authorize(Roles = "Admin")]
    // GET: Customers/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            TempData["ErrorMessage"] = "Customer not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Customer deleted successfully.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Unable to delete customer. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = "Admin")]
    private bool CustomerExists(int id)
    {
        return _context.Customers.Any(e => e.CustomerId == id);
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportToExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var customers = await _context.Customers.ToListAsync();

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Customers");

            // Header của file Excel
            worksheet.Cells[1, 1].Value = "Customer ID";
            worksheet.Cells[1, 2].Value = "Full Name";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Phone";
            worksheet.Cells[1, 5].Value = "Address";
            worksheet.Cells[1, 6].Value = "Notes";

            // Dữ liệu khách hàng
            int row = 2;
            foreach (var customer in customers)
            {
                worksheet.Cells[row, 1].Value = customer.CustomerId;
                worksheet.Cells[row, 2].Value = customer.FullName ?? string.Empty; // Nếu FullName là NULL, sử dụng chuỗi rỗng
                worksheet.Cells[row, 3].Value = customer.Email ?? string.Empty;
                worksheet.Cells[row, 4].Value = customer.Phone ?? string.Empty;
                worksheet.Cells[row, 5].Value = customer.Address ?? string.Empty;
                worksheet.Cells[row, 6].Value = customer.Notes ?? string.Empty;
                row++;
            }   

            // Định dạng cho file Excel
            worksheet.Cells[1, 1, row - 1, 6].AutoFitColumns();
            worksheet.Cells[1, 1, 1, 6].Style.Font.Bold = true;

            // Lưu tạm thời vào MemoryStream
            var stream = new MemoryStream();
            package.SaveAs(stream);

            // Trả về file Excel
            string fileName = "CustomerList.xlsx";
            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
