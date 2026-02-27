using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationApi.DTOs;
using AuthenticationApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationApi.Services
{
    /// <summary>
    /// Handles user registration, login, and JWT generation
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        public async Task RegisterAsync(RegisterDto request)
        {
            // Check if user exists by email or username
            var existingUser =
                await _userManager.FindByEmailAsync(request.Email)
                ?? await _userManager.FindByNameAsync(request.Username);

            if (existingUser != null)
            {
                throw new ArgumentException("User already exists");
            }

            var user = new User
            {
                UserName = request.Username,
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new ArgumentException(GetErrors(result.Errors));
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        public async Task<string> LoginAsync(LoginDto request)
        {
            // Find user by username or email
            var user =
                await _userManager.FindByNameAsync(request.Username)
                ?? await _userManager.FindByEmailAsync(request.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new ArgumentException("Invalid credentials");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            return GenerateJwtToken(claims);
        }

        /// <summary>
        /// Generates JWT token
        /// </summary>
        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!)
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddHours(3),
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Extracts error messages from IdentityError list
        /// </summary>
        private static string GetErrors(IEnumerable<IdentityError> errors)
        {
            return string.Join(", ", errors.Select(e => e.Description));
        }
    }
}