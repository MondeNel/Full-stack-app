using System.Text;
using AuthenticationApi.Data;
using AuthenticationApi.Entities;
using AuthenticationApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Entry point of the Authentication API application.
/// Configures services, middleware, and runs the app.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Provides access to configuration from appsettings.json
/// </summary>
var configuration = builder.Configuration;

#region Configure Services

/// <summary>
/// 1️⃣ Configure PostgreSQL DbContext using Entity Framework Core
/// This connects to the PostgreSQL database specified in appsettings.json.
/// </summary>
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

/// <summary>
/// 2️⃣ Configure ASP.NET Identity
/// Provides built-in user management, password hashing, and authentication tables.
/// Uses the AppDbContext for storing users and roles.
/// </summary>
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

/// <summary>
/// 3️⃣ Configure JWT Authentication
/// Sets up authentication to use JWT Bearer tokens.
/// Validates issuer, audience, and signing key from configuration.
/// </summary>
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;                 // Saves token for reuse
    options.RequireHttpsMetadata = false;     // For development; enable HTTPS in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,                // Ensure token is from a trusted issuer
        ValidateAudience = true,              // Ensure token is intended for our audience
        ValidIssuer = configuration["JWT:ValidIssuer"],
        ValidAudience = configuration["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!)) // Secret key for signing
    };
});

/// <summary>
/// 4️⃣ Register application services
/// Adds our custom AuthenticationService for dependency injection
/// </summary>
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

/// <summary>
/// 5️⃣ Add Controllers
/// Enables use of API controllers for routing requests
/// </summary>
builder.Services.AddControllers();

/// <summary>
/// 6️⃣ Configure CORS (Cross-Origin Resource Sharing)
/// Allows frontend development on localhost:3000 (React app)
/// to access the API without CORS issues
/// </summary>
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDevClient", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

#endregion

/// <summary>
/// Build the application with the configured services
/// </summary>
var app = builder.Build();

#region Configure Middleware

/// <summary>
/// Use CORS policy defined above
/// </summary>
app.UseCors("AllowReactDevClient");

/// <summary>
/// Redirect HTTP requests to HTTPS
/// </summary>
app.UseHttpsRedirection();

/// <summary>
/// Enable authentication middleware
/// Validates JWT tokens in incoming requests
/// </summary>
app.UseAuthentication();

/// <summary>
/// Enable authorization middleware
/// Checks user permissions for protected endpoints
/// </summary>
app.UseAuthorization();

/// <summary>
/// Map controller routes (API endpoints)
/// </summary>
app.MapControllers();

#endregion

/// <summary>
/// Run the application
/// </summary>
app.Run();