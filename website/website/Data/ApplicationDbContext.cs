using Microsoft.EntityFrameworkCore;
using website.Models;

namespace website.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Document> Documents { get; set; } // Thêm dòng này

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<Customer>().ToTable("customers");
            modelBuilder.Entity<CartItem>().ToTable("cart");
            modelBuilder.Entity<Invoice>().ToTable("invoices");
            modelBuilder.Entity<InvoiceItem>().ToTable("invoice_items");
            modelBuilder.Entity<ProductImage>().ToTable("product_images");
            modelBuilder.Entity<Document>().ToTable("documents"); // Thêm dòng này

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId);
        }
    }
}
