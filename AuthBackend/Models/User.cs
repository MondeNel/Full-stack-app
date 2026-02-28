using Microsoft.AspNetCore.Identity;

namespace AuthenticationApi.Models
{
    /// <summary>
    /// Represents an application user within the authentication system.
    /// Extends <see cref="IdentityUser"/> to leverage ASP.NET Core Identity
    /// for secure user management.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Gets or sets the UTC date and time when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Custom domain-specific fields can be added here if required.
        // Example:
        // public string FirstName { get; set; }
    }
}