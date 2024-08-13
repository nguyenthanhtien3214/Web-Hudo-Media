using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class QuanLyController : Controller
{
    [Authorize(Policy = "AdminOnly")]

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Products()
    {
        return RedirectToAction("Index", "Products");
    }

    public IActionResult Categories()
    {
        return RedirectToAction("Index", "Categories");
    }

    public IActionResult Customers()
    {
        return RedirectToAction("Index", "Customers");
    }

    public IActionResult InvoiceItems()
    {
        return RedirectToAction("Edit", "InvoiceItems");
    }
}
