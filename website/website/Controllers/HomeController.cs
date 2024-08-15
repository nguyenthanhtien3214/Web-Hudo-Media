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

            List<CartItem> cart = new List<CartItem>();
            string cartCookie = Request.Cookies["Cart"];

            if (!string.IsNullOrEmpty(cartCookie))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartCookie);
            }

            var cartItem = cart.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                if (cartItem.Quantity + quantity > product.Quantity)
                {
                    return Json(new { success = false, message = $"Số lượng tồn kho không đủ. Chỉ còn {product.Quantity} sản phẩm." });
                }
                cartItem.Quantity += quantity;
                cartItem.RentalDays += rentalDays;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    RentalDays = rentalDays,
                    Price = product.Price,
                    Product = product
                });
            }

            string updatedCart = JsonConvert.SerializeObject(cart);
            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7)
            };
            Response.Cookies.Append("Cart", updatedCart, options);

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

            List<CartItem> cart = new List<CartItem>();
            string cartCookie = Request.Cookies["Cart"];

            if (!string.IsNullOrEmpty(cartCookie))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartCookie);
            }

            var cartItem = cart.FirstOrDefault(ci => ci.ProductId == cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                cartItem.RentalDays = rentalDays;

                // Cập nhật giá tổng cộng dựa trên số lượng và số ngày thuê
                cartItem.Price = product.Price * quantity * rentalDays;

                string updatedCart = JsonConvert.SerializeObject(cart);
                CookieOptions options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7)
                };
                Response.Cookies.Append("Cart", updatedCart, options);

                UpdateCartCount();
                return Json(new { success = true, message = "Giỏ hàng đã được cập nhật thành công", cartCount = cart.Count });
            }

            return Json(new { success = false, message = "Không tìm thấy mục giỏ hàng" });
        }


        public IActionResult Cart()
        {
            List<CartItem> cart = new List<CartItem>();
            string cartCookie = Request.Cookies["Cart"];

            if (!string.IsNullOrEmpty(cartCookie))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartCookie);

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
            }

            return View(cart);
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

                // Lấy giỏ hàng từ cookie
                var cartCookie = Request.Cookies["Cart"];
                List<CartItem> cartItems = new List<CartItem>();

                if (!string.IsNullOrEmpty(cartCookie))
                {
                    cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartCookie);
                }

                if (cartItems == null || !cartItems.Any())
                {
                    return Json(new { success = false, message = "Giỏ hàng của bạn hiện đang trống." });
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

                    // Cập nhật số lượng sản phẩm
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

                // Tạo đối tượng InvoiceViewModel
                var invoiceViewModel = new InvoiceViewModel
                {
                    InvoiceId = invoice.InvoiceId,
                    CustomerName = customer.FullName,
                    CustomerEmail = customer.Email,
                    CustomerPhone = customer.Phone,
                    CustomerAddress = customer.Address,
                    CartItems = cartItems.Select(ci => new CartItemViewModel
                    {
                        ProductName = ci.Product.Name,
                        Quantity = ci.Quantity,
                        Price = ci.Price,
                        RentalDays = ci.RentalDays
                    }).ToList(),
                    TotalAmount = invoice.TotalAmount
                };

                // Tạo HTML cho các mục trong giỏ hàng
                var orderItemsHtml = new StringBuilder();
                foreach (var item in invoiceViewModel.CartItems)
                {
                    orderItemsHtml.AppendLine($"<tr><td>{item.ProductName}</td><td>{item.Quantity}</td><td>{item.Price:C}</td><td>{(item.Quantity * item.Price):C}</td></tr>");
                }

                // Đọc file HTML từ wwwroot/templates
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates/InvoiceTemplate.html");
                var emailBody = await System.IO.File.ReadAllTextAsync(templatePath);

                // Thay thế các placeholder trong template bằng dữ liệu thực
                emailBody = emailBody.Replace("{{InvoiceId}}", invoice.InvoiceId.ToString())
                                     .Replace("{{CustomerName}}", customer.FullName)
                                     .Replace("{{CustomerEmail}}", customer.Email)
                                     .Replace("{{CustomerPhone}}", customer.Phone)
                                     .Replace("{{CustomerAddress}}", customer.Address)
                                     .Replace("{{TotalAmount}}", invoice.TotalAmount.ToString("C"))
                                     .Replace("{{OrderItems}}", orderItemsHtml.ToString());

                try
                {
                    await _emailService.SendEmailAsync(customer.Email, "Xác nhận đơn hàng", emailBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending order confirmation email.");
                }

                // Xóa giỏ hàng sau khi gửi email thành công
                Response.Cookies.Delete("Cart");

                return RedirectToAction("Index");
            }

            return View(customer);
        }









        private void UpdateCartCount()
        {
            int cartCount = 0;
            string cartCookie = Request.Cookies["Cart"];

            if (!string.IsNullOrEmpty(cartCookie))
            {
                var cart = JsonConvert.DeserializeObject<List<CartItem>>(cartCookie);
                cartCount = cart.Count;
            }

            ViewBag.CartCount = cartCount;
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            List<CartItem> cart = new List<CartItem>();
            string cartCookie = Request.Cookies["Cart"];

            if (!string.IsNullOrEmpty(cartCookie))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartCookie);
            }

            var cartItem = cart.FirstOrDefault(ci => ci.ProductId == cartItemId);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                string updatedCart = JsonConvert.SerializeObject(cart);
                CookieOptions options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7)
                };
                Response.Cookies.Append("Cart", updatedCart, options);
            }

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
