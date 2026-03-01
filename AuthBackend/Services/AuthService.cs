using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationApi.DTOs;
using AuthenticationApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationApi.Services
{
    /// <summary>
    /// Provides authentication functionality including user registration,
    /// login, and JWT token generation.
    /// </summary>
    public class AuthService : IAuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="userManager">ASP.NET Identity user manager.</param>
        /// <param name="configuration">Application configuration settings.</param>
        public AuthService(
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user after validating uniqueness and hashing the password.
        /// </summary>
        /// <param name="request">User registration details.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a user with the same email or username already exists.
        /// </exception>
        public async Task RegisterAsync(RegisterDto request)
        {
            var existingUser =
                await _userManager.FindByEmailAsync(request.Email)
                ?? await _userManager.FindByNameAsync(request.Username);

            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists.");
            }

            var user = new User
            {
                UserName = request.Username,
                Email = request.Email,
                FirstName = string.Empty,  // Default to empty string
                LastName = string.Empty     // Default to empty string
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(GetErrors(result.Errors));
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT access token.
        /// </summary>
        /// <param name="request">User login credentials.</param>
        /// <returns>A JWT token string.</returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when credentials are invalid.
        /// </exception>
        public async Task<string> LoginAsync(LoginDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FirstName", user.FirstName),  // Added
                new Claim("LastName", user.LastName)     // Added
            };

            return GenerateJwtToken(claims);
        }

        /// <summary>
        /// Generates a signed JWT token containing user claims.
        /// </summary>
        /// <param name="claims">Claims to embed in the token.</param>
        /// <returns>Serialized JWT token.</returns>
        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var secret = _configuration["JWT:Secret"]
                ?? throw new InvalidOperationException("JWT Secret is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Aggregates identity errors into a readable string.
        /// </summary>
        /// <param name="errors">Collection of identity errors.</param>
        /// <returns>Combined error message.</returns>
        private static string GetErrors(IEnumerable<IdentityError> errors)
        {
            return string.Join(", ", errors.Select(e => e.Description));
        }
    }
}