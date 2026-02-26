using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.DTOs
{
    /// <summary>
    /// Data required to authenticate a user
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Username of the user
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}