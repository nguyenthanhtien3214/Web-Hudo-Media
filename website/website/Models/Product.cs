using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace website.Models
{
    public class Product
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("price", TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("image_url")]
        public string ImageUrl { get; set; }

        [Required]
        [Column("category_id")]
        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
