using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace website.Models
{
    [Table("product_images")]
    public class ProductImage
    {
        [Key]
        [Column("image_id")]
        public int ImageId { get; set; }

        [Required]
        [Column("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [Column("product_id")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
