using System.Text;
using AuthenticationApi.Data;
using AuthenticationApi.Models;
using AuthenticationApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.InMemory;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

#region Configure Services

// Configure DbContext - supports swapping for tests via environment
var connectionString = configuration.GetConnectionString("DefaultConnection");
var isTestEnvironment = builder.Environment.EnvironmentName == "Testing";

if (!isTestEnvironment)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}

// Configure ASP.NET Identity for User management
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSecret = configuration["JWT:Secret"]
    ?? throw new InvalidOperationException("JWT Secret is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = configuration["JWT:ValidIssuer"],
        ValidAudience = configuration["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// Register application services
builder.Services.AddScoped<IAuthenticationService, AuthService>();

// Add Controllers
builder.Services.AddControllers();

// Configure CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDevClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

#endregion

var app = builder.Build();

#region Configure Middleware

app.UseCors("AllowReactDevClient");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

#endregion

app.Run();

// Needed for integration tests
public partial class Program { }