using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace website.Models
{
    public class Category
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty; // Khởi tạo với giá trị mặc định

        public ICollection<Product>? Products { get; set; } = new List<Product>(); // Đánh dấu nullable và khởi tạo với giá trị mặc định
    }

}
