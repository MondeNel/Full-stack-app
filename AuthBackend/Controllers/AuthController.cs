using Microsoft.AspNetCore.Mvc;
using AuthBackend.Services;
using AuthBackend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user
        /// Returns user info + JWT token
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = await _authService.RegisterAsync(dto);
            if (user == null) return BadRequest(new { message = "Email already in use." });
            return Ok(user);
        }

        /// <summary>
        /// Login a user
        /// Returns user info + JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _authService.LoginAsync(dto);
            if (user == null) return Unauthorized(new { message = "Invalid credentials." });
            return Ok(user);
        }

        /// <summary>
        /// Get currently logged-in user info
        /// Requires Authorization header with Bearer token
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var firstName = User.FindFirstValue(ClaimTypes.Name);

            return Ok(new
            {
                Id = userId,
                FirstName = firstName,
                Email = email
            });
        }

        /// <summary>
        /// Get all users
        /// Requires Authorization header with Bearer token
        /// </summary>
        [HttpGet("users")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Update a user by ID
        /// Returns updated user info + new JWT token
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var updatedUser = await _authService.UpdateUserAsync(id, dto);
            if (updatedUser == null) return NotFound(new { message = "User not found." });
            return Ok(updatedUser);
        }

        /// <summary>
        /// Delete a user by ID
        /// Requires Authorization header with Bearer token
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _authService.DeleteUserAsync(id);
            if (!deleted) return NotFound(new { message = "User not found." });
            return Ok(new { message = "User deleted successfully." });
        }
    }
}