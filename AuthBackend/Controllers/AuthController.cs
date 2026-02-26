using AuthenticationApi.DTOs;
using AuthenticationApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="request">User login credentials</param>
        /// <returns>JWT token</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var token = await _authService.LoginAsync(request);
            return Ok(new { token });
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">User registration data</param>
        /// <returns>Success message</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            await _authService.RegisterAsync(request);
            return Ok(new { message = "User registered successfully" });
        }

        /// <summary>
        /// Protected test endpoint
        /// </summary>
        /// <returns>Authorized message</returns>
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new { message = "You are authorized" });
        }
    }
}