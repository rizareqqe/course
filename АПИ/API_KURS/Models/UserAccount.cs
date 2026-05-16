using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieLibrary.Models
{
    [Table("user_accounts")]
    public class UserAccount
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("login")]
        [MaxLength(50)]
        [Required]
        public string Login { get; set; } = string.Empty;

        [Column("password")]
        [MaxLength(100)]
        [Required]
        public string Password { get; set; } = string.Empty;

        [Column("full_name")]
        [MaxLength(150)]
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Column("role")]
        [MaxLength(30)]
        [Required]
        public string Role { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
