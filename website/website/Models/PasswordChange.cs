using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace website.Models
{
    public class PasswordChange
    {
        [Key]
        [Column("change_id")]
        public int ChangeId { get; set; }

        [ForeignKey("Admin")]
        [Column("admin_id")]
        public int AdminId { get; set; }

        [Required]
        [Column("old_password_hash")]
        public string OldPasswordHash { get; set; }

        [Required]
        [Column("new_password_hash")]
        public string NewPasswordHash { get; set; }

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; }

        public Admin Admin { get; set; }
    }
}
