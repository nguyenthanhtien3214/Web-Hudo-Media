using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using website.Models;
using website.Data;
using website.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace website.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            // Tạo tài khoản admin mặc định nếu chưa tồn tại
            if (!_context.Admins.Any(a => a.Username == "admin"))
            {
                var admin = new Admin
                {
                    Username = "admin",
                    Email = "nguyenthanhtien3214@gmail.com",
                    PasswordHash = ComputeSha256Hash("adminpassword"),
                    CreatedAt = DateTime.Now
                };
                _context.Admins.Add(admin);
                _context.SaveChanges();
            }
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var passwordHash = ComputeSha256Hash(password);
            var admin = _context.Admins
                .FirstOrDefault(a => a.Username == username && a.PasswordHash == passwordHash);

            if (admin == null)
            {
                ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            // Tạo Claims để xác thực người dùng
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, "Admin") // Thêm quyền Admin
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Ghi nhớ đăng nhập
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            };

            // Đăng nhập
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Chuyển hướng đến trang quản lý
            return RedirectToAction("Index", "QuanLy");
        }
        [Authorize(Roles = "Admin")]
        // Phương thức đăng xuất
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Lưu thông báo vào TempData
            TempData["LogoutMessage"] = "Bạn đã đăng xuất thành công.";

            // Chuyển hướng về trang Home/Index
            return RedirectToAction("Index", "Home");
        }
        [Authorize(Roles = "Admin")]
        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]

        // POST: /Account/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // Sử dụng email mặc định của admin
            string defaultEmail = "nguyenthanhtien3214@gmail.com";

            var admin = _context.Admins.FirstOrDefault(a => a.Email == defaultEmail);

            // Kiểm tra nếu admin là null
            if (admin == null)
            {
                // Thêm mã này để kiểm tra email trong cơ sở dữ liệu
                var allAdmins = _context.Admins.ToList(); // Lấy danh sách tất cả các admin để kiểm tra
                var emails = string.Join(", ", allAdmins.Select(a => a.Email)); // Liệt kê tất cả các email
                Console.WriteLine("Các email trong hệ thống: " + emails);

                ViewBag.ErrorMessage = "Email không tồn tại trong hệ thống.";
                return View();
            }

            // Nếu admin tồn tại, tiếp tục xử lý
            var token = Guid.NewGuid().ToString();
            var expiresAt = DateTime.Now.AddMinutes(15);

            _context.PasswordResets.Add(new PasswordReset
            {
                AdminId = admin.AdminId,
                Token = ComputeSha256Hash(token),
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();

            var resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);
            var subject = "Đặt lại mật khẩu của bạn";
            var body = $"Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu: {resetLink}";

            await _emailService.SendEmailAsync(admin.Email, subject, body);

            ViewBag.Message = "Đã gửi email hướng dẫn đặt lại mật khẩu.";
            return View();
        }
        [Authorize(Roles = "Admin")]
        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var resetRequest = _context.PasswordResets
                .FirstOrDefault(pr => pr.Token == ComputeSha256Hash(token) && pr.ExpiresAt > DateTime.Now);

            if (resetRequest == null)
            {
                ViewBag.ErrorMessage = "Token không hợp lệ hoặc đã hết hạn.";
                return View("Error");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }
        [Authorize(Roles = "Admin")]
        // POST: /Account/ResetPassword
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var resetRequest = _context.PasswordResets
                    .FirstOrDefault(pr => pr.Token == ComputeSha256Hash(model.Token) && pr.ExpiresAt > DateTime.Now);

                if (resetRequest == null)
                {
                    return RedirectToAction("Error");
                }

                var admin = _context.Admins.FirstOrDefault(a => a.AdminId == resetRequest.AdminId);

                if (admin != null)
                {
                    admin.PasswordHash = ComputeSha256Hash(model.NewPassword);
                    _context.SaveChanges();

                    return RedirectToAction("Success");
                }
            }

            ViewBag.ErrorMessage = "Đã có lỗi xảy ra. Vui lòng thử lại.";
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        [Authorize(Roles = "Admin")]
        // GET: /Account/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        // GET: /Account/Error
        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }
    }
}
