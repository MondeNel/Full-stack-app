using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.DTOs
{
    /// <summary>
    /// Represents the data required to authenticate a user.
    /// Used for input validation on the login endpoint.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Email address of the user.
        /// Example: "user@example.com"
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}