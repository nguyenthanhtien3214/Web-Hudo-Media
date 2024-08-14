using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace website.Models
{
    public class LoginAttempt
    {
        [Key]
        [Column("attempt_id")]
        public int AttemptId { get; set; }

        [ForeignKey("Admin")]
        [Column("admin_id")]
        public int AdminId { get; set; }

        [Required]
        [Column("ip_address")]
        public string IpAddress { get; set; }

        [Column("attempted_at")]
        public DateTime AttemptedAt { get; set; }

        [Column("status")]
        public string Status { get; set; }

        public Admin Admin { get; set; }
    }
}
