using System.Security.Claims;
using AuthenticationApi.DTOs;
using AuthenticationApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationApi.Controllers
{
    /// <summary>
    /// Handles authentication-related operations such as user registration,
    /// login, and retrieval of authenticated user details.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">Authentication service responsible for business logic.</param>
        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates a user using email/username and password
        /// and returns a JWT token if credentials are valid.
        /// </summary>
        /// <param name="request">User login credentials.</param>
        /// <returns>A JWT access token.</returns>
        /// <response code="200">Returns the JWT token.</response>
        /// <response code="401">If the credentials are invalid.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var token = await _authService.LoginAsync(request);
                return Ok(new { accessToken = token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">User registration details.</param>
        /// <returns>A success message.</returns>
        /// <response code="200">User registered successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _authService.RegisterAsync(request);
                return Ok(new { message = "User registered successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves details of the currently authenticated user
        /// based on the JWT token provided in the Authorization header.
        /// </summary>
        /// <returns>The authenticated user's basic information.</returns>
        /// <response code="200">Returns user details.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var firstName = User.FindFirstValue("FirstName");  // Added
            var lastName = User.FindFirstValue("LastName");    // Added

            return Ok(new
            {
                id = userId,
                email,
                firstName,  // Added
                lastName    // Added
            });
        }
    }
}