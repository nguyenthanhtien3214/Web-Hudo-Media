namespace website.Models
{
    public class InvoiceViewModel
    {
        public int InvoiceId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } // Danh sách các mục trong giỏ hàng
        public decimal TotalAmount { get; set; }
    }

    public class CartItemViewModel // Đổi tên lại cho phù hợp với vai trò của nó
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int RentalDays { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price; // Hoặc tính toán riêng trong controller nếu cần
    }
}
