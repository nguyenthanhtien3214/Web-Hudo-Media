using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace website.Models
{
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [Column("name")]
        public string Name { get; set; } = string.Empty; // Khởi tạo với giá trị mặc định

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm.")]
        [Column("description")]
        public string Description { get; set; } = string.Empty; // Khởi tạo với giá trị mặc định

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        [Column("price")]
        public decimal Price { get; set; }

        [Column("image")]
        public string? Image { get; set; } // Đánh dấu nullable

        [Column("image_url")]
        public string? ImageUrl { get; set; } // Đánh dấu nullable

        [Required(ErrorMessage = "Vui lòng nhập số lượng sản phẩm.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn 0.")]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục sản phẩm.")]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; } // Đánh dấu nullable
    }
}
