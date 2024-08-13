using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace website.Models
{
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [Column("full_name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        [Column("email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ.")]
        [Column("phone")]
        public string Phone { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("notes")]
        public string Notes { get; set; }
    }
}