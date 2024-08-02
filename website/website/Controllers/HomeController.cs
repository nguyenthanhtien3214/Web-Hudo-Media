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


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, EmailService emailService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;

        }

        public IActionResult Gallery()
        {
            return View();
        }

        public IActionResult Index()
        {
            UpdateCartCount();
            return View();
        }

        public IActionResult DichVuThue(int? page)
        {
            UpdateCartCount();
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng sản phẩm trên mỗi trang

            var products = _context.Products
                .Include(p => p.Images) // Đảm bảo bao gồm hình ảnh
                .ToPagedList(pageNumber, pageSize);

            var model = new ProductViewModel
            {
                Products = products
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

            List<CartItem> cart = new List<CartItem>();
            string cartCookie = Request.Cookies["Cart"];

            if (!string.IsNullOrEmpty(cartCookie))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartCookie);
            }

            var cartItem = cart.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
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
                _context.SaveChanges();

                // Lấy giỏ hàng từ cookie
                var cartCookie = Request.Cookies["Cart"];
                List<CartItem> cartItems = new List<CartItem>();

                if (!string.IsNullOrEmpty(cartCookie))
                {
                    cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartCookie);
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
                _context.SaveChanges();

                // Thêm các mục vào hóa đơn
                foreach (var item in cartItems)
                {
                    var invoiceItem = new InvoiceItem
                    {
                        InvoiceId = invoice.InvoiceId,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        RentalDays = item.RentalDays,
                        Price = item.Price,
                        Total = item.Quantity * item.Price
                    };

                    _context.InvoiceItems.Add(invoiceItem);
                }

                _context.SaveChanges();

                // Gửi email xác nhận
                var emailBody = new StringBuilder();
                emailBody.AppendLine("<h1>Hóa đơn của bạn</h1>");
                emailBody.AppendLine($"<p>Hóa đơn ID: {invoice.InvoiceId}</p>");
                emailBody.AppendLine($"<p>Họ và tên: {customer.FullName}</p>");
                emailBody.AppendLine($"<p>Email: {customer.Email}</p>");
                emailBody.AppendLine($"<p>Phone: {customer.Phone}</p>");
                emailBody.AppendLine($"<p>Address: {customer.Address}</p>");
                emailBody.AppendLine($"<p>Notes: {customer.Notes}</p>");
                emailBody.AppendLine("<h2>Chi tiết hóa đơn</h2>");
                emailBody.AppendLine("<table border='1'><tr><th>Sản phẩm</th><th>Số lượng</th><th>Số ngày thuê</th><th>Đơn giá</th><th>Tổng</th></tr>");

                foreach (var item in cartItems)
                {
                    emailBody.AppendLine($"<tr><td>{item.Product.Name}</td><td>{item.Quantity}</td><td>{item.RentalDays}</td><td>{item.Price.ToString("C")}</td><td>{(item.Quantity * item.Price).ToString("C")}</td></tr>");
                }

                emailBody.AppendLine("</table>");
                emailBody.AppendLine($"<h3>Tổng cộng: {invoice.TotalAmount.ToString("C")}</h3>");

                await _emailService.SendEmailAsync(customer.Email, "Xác nhận đơn hàng", emailBody.ToString());

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
            }

            return View();
        }



    }
}
