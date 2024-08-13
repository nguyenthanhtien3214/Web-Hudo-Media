using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace website.Models
{
    public class InvoiceItem
    {
        [Key]
        [Column("invoice_item_id")]
        public int InvoiceItemId { get; set; }

        [Column("invoice_id")]
        public int InvoiceId { get; set; }

        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Column("product_name")]
        [Required(ErrorMessage = "Product name is required")]
        public string ProductName { get; set; } // Bắt buộc, không cho phép NULL

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("rental_days")]
        public int RentalDays { get; set; }


        [Column("price")]
        public decimal Price { get; set; }

        [Column("total")]
        public decimal Total { get; set; }

    }
}
