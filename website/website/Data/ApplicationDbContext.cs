﻿using Microsoft.EntityFrameworkCore;
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
        public DbSet<Document> Documents { get; set; }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<PasswordChange> PasswordChanges { get; set; }

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
            modelBuilder.Entity<Document>().ToTable("documents");

            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<PasswordReset>().ToTable("PasswordResets");
            modelBuilder.Entity<PasswordChange>().ToTable("PasswordChanges");
            modelBuilder.Entity<LoginAttempt>().ToTable("LoginAttempts");

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId);

            // Configure data types
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.Quantity);

            modelBuilder.Entity<CartItem>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<InvoiceItem>()
                .Property(ii => ii.Price)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<InvoiceItem>()
                .Property(ii => ii.Total)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Admin>()
                .Property(a => a.CreatedAt)
                .HasColumnType("datetime2");

            modelBuilder.Entity<Admin>()
                .Property(a => a.UpdatedAt)
                .HasColumnType("datetime2");

            modelBuilder.Entity<PasswordReset>()
                .Property(pr => pr.CreatedAt)
                .HasColumnType("datetime2");

            modelBuilder.Entity<PasswordReset>()
                .Property(pr => pr.ExpiresAt)
                .HasColumnType("datetime2");

            modelBuilder.Entity<PasswordChange>()
                .Property(pc => pc.ChangedAt)
                .HasColumnType("datetime2");

            modelBuilder.Entity<LoginAttempt>()
                .Property(la => la.AttemptedAt)
                .HasColumnType("datetime2");
        }
    }
}
