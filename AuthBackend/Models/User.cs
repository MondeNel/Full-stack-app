using System.ComponentModel.DataAnnotations;

namespace AuthBackend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}