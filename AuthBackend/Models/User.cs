using Microsoft.AspNetCore.Identity;

namespace AuthenticationApi.Entities
{
    /// <summary>
    /// Application user entity extending ASP.NET Identity
    /// </summary>
    public class User : IdentityUser
    {
        // Add custom properties here if needed in the future
        // Example:
        // public string FirstName { get; set; }
    }
}