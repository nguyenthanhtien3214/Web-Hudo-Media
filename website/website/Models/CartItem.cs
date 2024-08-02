using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace website.Models
{
    public class CartItem
    {
        [Key]
        [Column("cart_id")]
        public int CartItemId { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("rental_days")]
        public int RentalDays { get; set; }

        [Required]
        [Column("price")]
        public decimal Price { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }
    }
}
