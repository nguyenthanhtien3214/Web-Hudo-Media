using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using website.Data;
using website.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

// Add DbContext service
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add EmailService
builder.Services.AddScoped<EmailService>(); // Đăng ký EmailService như một dịch vụ có phạm vi

// Add ViewRenderService
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn khi truy cập bị từ chối
    });

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Add Session service
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Đảm bảo session hoạt động ngay cả khi cookie là tùy chọn
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Use static files (e.g., for serving CSS, JS, images, etc.)
app.UseStaticFiles();

// Enable session
app.UseSession(); // Thêm Middleware cho Session

// Routing
app.UseRouting();

// Authentication & Authorization Middleware
app.UseAuthentication(); // Thêm Middleware Authentication
app.UseAuthorization();  // Thêm Middleware Authorization

// Map routes for MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
