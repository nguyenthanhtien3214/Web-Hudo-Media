using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace website.Models
{
    public class Invoice
    {
        [Key]
        [Column("invoice_id")]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "Mã khách hàng không được để trống.")]
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Tên khách hàng không được để trống.")]
        [Column("full_name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [Column("email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [Column("phone")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        [Column("address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Ghi chú không được để trống.")]
        [Column("notes")]
        public string Notes { get; set; }

        [Required(ErrorMessage = "Tổng tiền không được để trống.")]
        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Ngày tạo không được để trống.")]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ngày bắt đầu thuê không được để trống.")]
        [Column("rental_start_date")]
        public DateTime RentalStartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc thuê không được để trống.")]
        [Column("rental_end_date")]
        public DateTime RentalEndDate { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [Column("status")]
        public string Status { get; set; } = "Chưa xác nhận";

        public ICollection<InvoiceItem> InvoiceItems { get; set; }
    }
}
