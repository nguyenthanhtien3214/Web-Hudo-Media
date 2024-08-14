using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using website.Models;
using X.PagedList;

namespace website.Models
{
    public class ProductViewModel
    {
        public Product NewProduct { get; set; }
        public IPagedList<Product> Products { get; set; }
        public List<IFormFile> Files { get; set; } // Danh sách các tệp hình ảnh

        // Thêm các thuộc tính mới
        public IEnumerable<Category> Categories { get; set; } // Danh sách các danh mục
        public int? SelectedCategoryId { get; set; } // Lưu trữ ID của danh mục đã chọn
    }
}
