using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthBackend.Models
{
    [Table("Users")] // Maps to your PostgreSQL table name
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("first_name")]
        public string FirstName { get; set; } = default!;

        [Required]
        [Column("last_name")]
        public string LastName { get; set; } = default!;

        [Required]
        [Column("email")]
        public string Email { get; set; } = default!;

        [Required]
        [Column("password")]
        public string PasswordHash { get; set; } = default!;

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}