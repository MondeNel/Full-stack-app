using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.DTOs
{
    /// <summary>
    /// Represents the data required to register a new user.
    /// Used for input validation on the registration endpoint.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// User's first name.
        /// Example: "John"
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name.
        /// Example: "Doe"
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Unique username chosen by the user.
        /// Example: "johndoe"
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's email address.
        /// Example: "user@example.com"
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password (minimum 6 characters).
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }
}