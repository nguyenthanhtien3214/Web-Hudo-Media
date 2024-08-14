using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class QuanLyController : Controller
{
    [Authorize(Roles = "Admin")]
    public IActionResult Index()
    {
        return View();
    }
    [Authorize(Roles = "Admin")]
    public IActionResult Products()
    {
        return RedirectToAction("Index", "Products");
    }
    [Authorize(Roles = "Admin")]
    public IActionResult Categories()
    {
        return RedirectToAction("Index", "Categories");
    }
    [Authorize(Roles = "Admin")]
    public IActionResult Customers()
    {
        return RedirectToAction("Index", "Customers");
    }
    [Authorize(Roles = "Admin")]
    public IActionResult InvoiceItems()
    {
        return RedirectToAction("Edit", "InvoiceItems");
    }
}
