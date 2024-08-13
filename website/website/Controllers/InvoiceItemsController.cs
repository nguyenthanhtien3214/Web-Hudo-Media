using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website.Models;
using System.Threading.Tasks;
using website.Data;

namespace website.Controllers
{
    public class InvoiceItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InvoiceItems
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách các hóa đơn cùng với các mục chi tiết
            var invoices = await _context.Invoices
                .Include(i => i.InvoiceItems) // Bao gồm các InvoiceItems liên quan
                .ToListAsync();

            // Lọc ra các hóa đơn không có mục chi tiết nào
            var invoicesToDelete = invoices.Where(i => !i.InvoiceItems.Any()).ToList();

            // Xóa các hóa đơn không có mục chi tiết nào
            if (invoicesToDelete.Any())
            {
                _context.Invoices.RemoveRange(invoicesToDelete);
                await _context.SaveChangesAsync();
            }

            // Cập nhật tổng tiền của các hóa đơn còn lại
            foreach (var invoice in invoices)
            {
                if (invoice.InvoiceItems.Any())
                {
                    invoice.TotalAmount = invoice.InvoiceItems.Sum(ii => ii.Total);
                }
            }

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Lấy danh sách hóa đơn đã cập nhật (không bao gồm hóa đơn đã xóa)
            invoices = await _context.Invoices.ToListAsync();

            return View(invoices);
        }

        // GET: InvoiceItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }
            return View(invoice);
        }

        // POST: InvoiceItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InvoiceId,CustomerId,FullName,Email,Phone,Address,Notes,TotalAmount,CreatedAt,RentalStartDate,RentalEndDate,Status")] Invoice invoice)
        {
            if (id != invoice.InvoiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý giá trị rỗng cho Notes nếu cần thiết
                    if (string.IsNullOrEmpty(invoice.Notes))
                    {
                        invoice.Notes = "Không có ghi chú";
                    }

                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoiceExists(invoice.InvoiceId))
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
            return View(invoice);
        }
        public async Task<IActionResult> Details(int? invoiceId)
        {
            if (invoiceId == null)
            {
                return NotFound();
            }

            // Lấy danh sách InvoiceItems dựa trên InvoiceId
            var invoiceItems = await _context.InvoiceItems
                .Where(i => i.InvoiceId == invoiceId)
                .Include(i => i.Product) // Nếu bạn cần thông tin sản phẩm
                .ToListAsync();

            if (invoiceItems == null || invoiceItems.Count == 0)
            {
                return NotFound();
            }

            return View(invoiceItems);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int invoiceItemId, int quantity)
        {
            var invoiceItem = await _context.InvoiceItems
        .Include(ii => ii.Product)
        .FirstOrDefaultAsync(ii => ii.InvoiceItemId == invoiceItemId);

            if (invoiceItem == null)
            {
                return NotFound();
            }

            // Kiểm tra số lượng tồn kho
            if (quantity > invoiceItem.Product.Quantity + invoiceItem.Quantity)
            {
                TempData["ErrorMessage"] = $"Số lượng tồn kho không đủ. Chỉ còn {invoiceItem.Product.Quantity} sản phẩm.";
                return RedirectToAction(nameof(Details), new { invoiceId = invoiceItem.InvoiceId });
            }

            // Khôi phục lại số lượng sản phẩm gốc trước khi cập nhật
            invoiceItem.Product.Quantity += invoiceItem.Quantity;

            // Cập nhật lại số lượng cho invoiceItem và trừ đi số lượng mới từ kho
            invoiceItem.Quantity = quantity;
            invoiceItem.Product.Quantity -= quantity;

            // Tính toán lại tổng cộng
            invoiceItem.Total = quantity * invoiceItem.Price;

            // Cập nhật lại cơ sở dữ liệu
            _context.Update(invoiceItem);
            await _context.SaveChangesAsync();

            // Đặt thông báo cập nhật thành công
            TempData["SuccessMessage"] = "Cập nhật thành công!";

            // Điều hướng về trang chi tiết hóa đơn
            return RedirectToAction(nameof(Details), new { invoiceId = invoiceItem.InvoiceId });
        }




            [HttpPost]
        public async Task<IActionResult> DeleteItem(int invoiceItemId)
        {
            var invoiceItem = await _context.InvoiceItems
        .Include(ii => ii.Invoice)
        .FirstOrDefaultAsync(ii => ii.InvoiceItemId == invoiceItemId);

            if (invoiceItem == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm để xóa.";
                return RedirectToAction(nameof(Index));
            }

            var invoiceId = invoiceItem.InvoiceId;
            _context.InvoiceItems.Remove(invoiceItem);
            await _context.SaveChangesAsync();

            // Kiểm tra nếu đây là sản phẩm cuối cùng trong hóa đơn
            var remainingItems = await _context.InvoiceItems
                .Where(ii => ii.InvoiceId == invoiceId)
                .ToListAsync();

            if (!remainingItems.Any())
            {
                // Nếu không còn sản phẩm nào trong hóa đơn, xóa luôn hóa đơn
                var invoice = await _context.Invoices.FindAsync(invoiceId);
                if (invoice != null)
                {
                    _context.Invoices.Remove(invoice);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Xóa sản phẩm thành công và hóa đơn đã được xóa vì không còn sản phẩm nào.";
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["SuccessMessage"] = "Xóa sản phẩm thành công.";
            return RedirectToAction(nameof(Details), new { invoiceId = invoiceId });
        }


        

        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.InvoiceId == id);
        }
    }
}
