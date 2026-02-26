using AuthBackend.Data;
using AuthBackend.Services;
using AuthBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AuthService
builder.Services.AddScoped<AuthService>();

// Add controllers
builder.Services.AddControllers();

// Read JWT settings safely
var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyString = jwtSettings.GetValue<string>("Key") 
                ?? throw new Exception("JWT Key not configured");
var key = Encoding.ASCII.GetBytes(keyString);

var issuer = jwtSettings.GetValue<string>("Issuer") 
             ?? throw new Exception("JWT Issuer not configured");
var audience = jwtSettings.GetValue<string>("Audience") 
               ?? throw new Exception("JWT Audience not configured");

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Add authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Apply migrations and seed default admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Apply pending migrations (if any)
    db.Database.Migrate();


}

// Configure middleware
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Run app
app.Run();