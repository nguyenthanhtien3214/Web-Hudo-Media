using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using website.Data;
using website.Models;
using website.Services;
using X.PagedList;
using X.PagedList.Extensions;

namespace website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly IViewRenderService _viewRenderService;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, EmailService emailService, IViewRenderService viewRenderService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
            _viewRenderService = viewRenderService;


        }

        public IActionResult Gallery()
        {
            return View();
        }

        public IActionResult Index()
        {
            ViewBag.CompanyIntroductionPart1 = "Hudo Media cung cấp dịch vụ cho thuê studio chuyên nghiệp với không gian hiện đại, đầy đủ trang thiết bị ánh sáng và âm thanh chất lượng cao. Chúng tôi đáp ứng nhu cầu quay phim, chụp ảnh, livestream với các gói dịch vụ linh hoạt, phù hợp với mọi yêu cầu của khách hàng.";
            ViewBag.CompanyIntroductionPart2 = "Đội ngũ kỹ thuật viên giàu kinh nghiệm luôn sẵn sàng hỗ trợ và đảm bảo quá trình làm việc diễn ra suôn sẻ. Hudo Media cam kết mang đến không gian sáng tạo tối ưu, giúp khách hàng thực hiện các dự án truyền thông một cách hiệu quả và chuyên nghiệp.";
            UpdateCartCount();
            return View();
        }

        public IActionResult DichVuThue(int? page, int? categoryId)
        {
            UpdateCartCount();
            int pageNumber = page ?? 1;
            int pageSize = 8; // Số lượng sản phẩm trên mỗi trang

            // Lấy tất cả danh mục để đổ vào combobox
            var categories = _context.Categories.ToList();

            var productsQuery = _context.Products
                .Include(p => p.Images)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = productsQuery.ToPagedList(pageNumber, pageSize);

            var model = new ProductViewModel
            {
                Products = products,
                Categories = categories,
                SelectedCategoryId = categoryId
            };

            return View(model);
        }

        public IActionResult ProductsCard(int id)
        {
            UpdateCartCount();

            // Nạp sản phẩm cùng với ảnh và danh mục
            var product = _context.Products
                .Include(p => p.Images) // Nạp ảnh liên quan đến sản phẩm
                .Include(p => p.Category) // Nạp danh mục liên quan
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity, int rentalDays)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            if (quantity > product.Quantity)
            {
                return Json(new { success = false, message = $"Số lượng tồn kho không đủ. Chỉ còn {product.Quantity} sản phẩm." });
            }

            // Lấy giỏ hàng từ session
            List<CartItem> cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                // Kiểm tra tồn kho trước khi cập nhật
                if (cartItem.Quantity + quantity > product.Quantity)
                {
                    return Json(new { success = false, message = $"Số lượng tồn kho không đủ. Chỉ còn {product.Quantity} sản phẩm." });
                }
                cartItem.Quantity += quantity;
                cartItem.RentalDays = rentalDays; // Chỉ cập nhật số ngày thuê, không cộng dồn
            }
            else
            {
                // Thêm sản phẩm mới vào giỏ hàng
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    RentalDays = rentalDays,
                    Price = product.Price, // Lưu đơn giá sản phẩm, không ghi đè sau đó
                    Product = product
                });
            }

            // Lưu giỏ hàng vào session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // Cập nhật tổng số lượng sản phẩm
            UpdateCartCount();

            return Json(new { success = true, message = "Thêm vào giỏ hàng thành công", cartCount = cart.Count });
        }




        [HttpPost]
        public IActionResult UpdateCart(int cartItemId, int quantity, int rentalDays)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == cartItemId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            if (quantity > product.Quantity)
            {
                return Json(new { success = false, message = $"Số lượng tồn kho không đủ. Chỉ còn {product.Quantity} sản phẩm." });
            }

            // Lấy giỏ hàng từ session
            List<CartItem> cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(ci => ci.ProductId == cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                cartItem.RentalDays = rentalDays;

                // Không cần cập nhật lại giá ở đây, giữ nguyên giá đã lưu
                // cartItem.Price = product.Price;

                // Lưu lại giỏ hàng trong session
                HttpContext.Session.SetObjectAsJson("Cart", cart);

                UpdateCartCount();
                return Json(new { success = true, message = "Giỏ hàng đã được cập nhật thành công", cartCount = cart.Sum(ci => ci.Quantity) });
            }

            return Json(new { success = false, message = "Không tìm thấy mục giỏ hàng" });
        }


        public IActionResult Cart()
        {
            // Lấy giỏ hàng từ session
            List<CartItem> cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Nạp thông tin sản phẩm từ cơ sở dữ liệu
            foreach (var item in cart)
            {
                var product = _context.Products
                    .Include(p => p.Images) // Nạp ảnh sản phẩm
                    .Include(p => p.Category) // Nạp danh mục sản phẩm
                    .FirstOrDefault(p => p.ProductId == item.ProductId);

                if (product != null)
                {
                    item.Product = product;
                    // Cập nhật giá tổng cộng dựa trên số lượng và số ngày thuê
                    item.Price = product.Price; // Đơn giá cho 1 sản phẩm trong 1 ngày
                }
            }

            return View(cart);
        }

        private void UpdateCartCount()
        {
            int cartCount = 0;

            // Lấy giỏ hàng từ session
            List<CartItem> cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Đếm tổng số lượng sản phẩm
            cartCount = cart.Sum(item => item.Quantity);

            ViewBag.CartCount = cartCount;
        }


        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Thêm thông tin khách hàng vào cơ sở dữ liệu
                _context.Customers.Add(customer);

                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving customer information to the database.");
                    return Json(new { success = false, message = "Đã xảy ra lỗi khi lưu thông tin khách hàng vào cơ sở dữ liệu." });
                }

                // Lấy giỏ hàng từ Session
                var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

                // Kiểm tra nếu giỏ hàng trống
                if (!cartItems.Any())
                {
                    TempData["EmptyCartMessage"] = "Vui lòng chọn sản phẩm rồi quay lại đây!";
                    return RedirectToAction("Cart", "Home");
                }

                // Tạo hóa đơn mới
                var invoice = new Invoice
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    Notes = customer.Notes,
                    TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Price),
                    CreatedAt = DateTime.Now
                };

                _context.Invoices.Add(invoice);

                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving invoice to the database.");
                    return Json(new { success = false, message = "Đã xảy ra lỗi khi lưu hóa đơn vào cơ sở dữ liệu." });
                }

                // Thêm các mục vào hóa đơn và cập nhật số lượng sản phẩm
                foreach (var item in cartItems)
                {
                    var product = _context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);

                    var invoiceItem = new InvoiceItem
                    {
                        InvoiceId = invoice.InvoiceId,
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        RentalDays = item.RentalDays,
                        Price = item.Price,
                        Total = item.Quantity * item.Price
                    };

                    _context.InvoiceItems.Add(invoiceItem);

                    // Cập nhật số lượng sản phẩm trong kho
                    product.Quantity -= item.Quantity;
                }

                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving invoice items to the database.");
                    return Json(new { success = false, message = "Đã xảy ra lỗi khi lưu chi tiết hóa đơn vào cơ sở dữ liệu." });
                }

                // Tạo HTML cho các mục đơn hàng
                var orderItemsHtml = new StringBuilder();
                foreach (var item in cartItems)
                {
                    orderItemsHtml.AppendLine($"<tr>");
                    orderItemsHtml.AppendLine($"<td>{item.Product.Name}</td>");
                    orderItemsHtml.AppendLine($"<td>{item.Quantity}</td>");
                    orderItemsHtml.AppendLine($"<td>{item.Price:C}</td>");
                    orderItemsHtml.AppendLine($"<td>{(item.Quantity * item.Price):C}</td>");
                    orderItemsHtml.AppendLine($"</tr>");
                }

                // Đọc template HTML từ file
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates/InvoiceTemplate.html");
                var emailBody = await System.IO.File.ReadAllTextAsync(templatePath);

                // Thay thế các placeholder trong template bằng dữ liệu thực tế
                emailBody = emailBody.Replace("{{InvoiceId}}", invoice.InvoiceId.ToString())
                                     .Replace("{{CustomerName}}", customer.FullName)
                                     .Replace("{{CustomerEmail}}", customer.Email)
                                     .Replace("{{CustomerPhone}}", customer.Phone)
                                     .Replace("{{CustomerAddress}}", customer.Address)
                                     .Replace("{{TotalAmount}}", invoice.TotalAmount.ToString("C"))
                                     .Replace("{{OrderItems}}", orderItemsHtml.ToString());

                // Gửi email xác nhận đơn hàng
                try
                {
                    await _emailService.SendEmailAsync(customer.Email, "Xác nhận đơn hàng", emailBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending order confirmation email.");
                }

                // Sau khi thanh toán thành công, xóa giỏ hàng khỏi Session
                HttpContext.Session.Remove("Cart");

                return RedirectToAction("Index");
            }

            return View(customer);
        }





        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            // Lấy giỏ hàng từ session
            List<CartItem> cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Tìm sản phẩm trong giỏ hàng theo ID
            var cartItem = cart.FirstOrDefault(ci => ci.ProductId == cartItemId);
            if (cartItem != null)
            {
                // Xóa sản phẩm khỏi giỏ hàng
                cart.Remove(cartItem);

                // Cập nhật lại giỏ hàng trong session
                HttpContext.Session.SetObjectAsJson("Cart", cart);

                // Cập nhật tổng số lượng sản phẩm trong giỏ
                UpdateCartCount();
            }

            // Chuyển hướng về trang giỏ hàng
            return RedirectToAction("Cart");
        }



        public IActionResult TrackOrder(string invoiceId)
        {
            if (string.IsNullOrEmpty(invoiceId))
            {
                return View();
            }

            // Tìm hóa đơn và các chi tiết của hóa đơn
            var invoice = _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(ii => ii.Product)
                .FirstOrDefault(i => i.InvoiceId.ToString() == invoiceId);

            if (invoice == null)
            {
                ViewBag.Message = "Không tìm thấy hóa đơn với ID đã nhập.";
                ViewBag.OrderDetails = null;
                ViewBag.TotalAmount = 0;
                ViewBag.Customer = null;
                ViewBag.RentalStartDate = null;
                ViewBag.RentalEndDate = null;
                ViewBag.Status = null;
            }
            else
            {
                ViewBag.Message = null;
                ViewBag.OrderDetails = invoice.InvoiceItems.Select(ii => new
                {
                    ii.ProductName,
                    ii.Quantity,
                    ii.RentalDays,
                    ii.Price
                }).ToList();
                ViewBag.TotalAmount = invoice.TotalAmount;
                ViewBag.Customer = new
                {
                    FullName = invoice.FullName,
                    Phone = invoice.Phone,
                    Address = invoice.Address
                };
                ViewBag.RentalStartDate = invoice.RentalStartDate.ToString("dd/MM/yyyy");
                ViewBag.RentalEndDate = invoice.RentalEndDate.ToString("dd/MM/yyyy");
                ViewBag.Status = invoice.Status;
            }

            return View();
        }




    }
}
