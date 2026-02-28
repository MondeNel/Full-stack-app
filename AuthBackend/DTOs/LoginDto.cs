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
        /// Username or email of the user.
        /// Example: "johndoe" or "user@example.com"
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's password.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}