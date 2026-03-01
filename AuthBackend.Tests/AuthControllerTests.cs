using AuthenticationApi.DTOs;
using AuthenticationApi.Services;
using AuthenticationApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AuthenticationApi.Tests
{
    /// <summary>
    /// Unit tests for <see cref="AuthController"/>.
    /// Verifies HTTP responses for register, login, and user detail endpoints.
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly AuthController _controller;

        /// <summary>
        /// Initializes mocks and creates a fresh <see cref="AuthController"/> instance
        /// before each test.
        /// </summary>
        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthenticationService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        // ─── Register Tests ────────────────────────────────────────────────

        /// <summary>
        /// Verifies that <see cref="AuthController.Register"/> returns
        /// <see cref="OkObjectResult"/> when registration succeeds.
        /// </summary>
        [Fact]
        public async Task Register_ValidRequest_ReturnsOk()
        {
            // Arrange
            _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
                .Returns(Task.CompletedTask);

            var dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john@example.com",
                Password = "password123"
            };

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        /// <summary>
        /// Verifies that <see cref="AuthController.Register"/> returns
        /// <see cref="BadRequestObjectResult"/> when user already exists.
        /// </summary>
        [Fact]
        public async Task Register_UserAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
                .ThrowsAsync(new InvalidOperationException("User already exists."));

            var dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john@example.com",
                Password = "password123"
            };

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ─── Login Tests ───────────────────────────────────────────────────

        /// <summary>
        /// Verifies that <see cref="AuthController.Login"/> returns
        /// <see cref="OkObjectResult"/> with a token when credentials are valid.
        /// </summary>
        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync("fake-jwt-token");

            var dto = new LoginDto
            {
                Username = "johndoe",
                Password = "password123"
            };

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        /// <summary>
        /// Verifies that <see cref="AuthController.Login"/> returns
        /// <see cref="UnauthorizedObjectResult"/> when credentials are invalid.
        /// </summary>
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

            var dto = new LoginDto
            {
                Username = "johndoe",
                Password = "wrongpassword"
            };

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}