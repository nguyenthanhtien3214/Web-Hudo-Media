using System.Collections.Generic;
using website.Models;
using X.PagedList;
using System.ComponentModel.DataAnnotations; // Thêm dòng này

namespace website.Models
{
    public class ProductViewModel
    {
        public Product NewProduct { get; set; }
        public IPagedList<Product> Products { get; set; }
    }
}
