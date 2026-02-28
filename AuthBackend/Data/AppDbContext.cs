using AuthenticationApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi.Data
{
    /// <summary>
    /// Represents the application's database context.
    /// Integrates ASP.NET Core Identity with Entity Framework Core
    /// and PostgreSQL for persistent storage.
    /// </summary>
    public class AppDbContext : IdentityDbContext<User>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppDbContext"/> class.
        /// </summary>
        /// <param name="options">Database context configuration options.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Configures the entity mappings and relationships.
        /// </summary>
        /// <param name="builder">The model builder used to configure entities.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Future customizations can be applied here
            // Example: renaming Identity tables or adding constraints
        }
    }
}