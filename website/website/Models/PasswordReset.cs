using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace website.Models
{
    public class PasswordReset
    {
        [Key]
        [Column("reset_id")]
        public int ResetId { get; set; }

        [ForeignKey("Admin")]
        [Column("admin_id")]
        public int AdminId { get; set; }

        [Required]
        [Column("token")]
        public string Token { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public Admin Admin { get; set; }
    }
}
