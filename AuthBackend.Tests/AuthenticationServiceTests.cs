using AuthenticationApi.DTOs;
using AuthenticationApi.Services;
using AuthenticationApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace AuthenticationApi.Tests
{
    /// <summary>
    /// Unit tests for <see cref="AuthService"/>.
    /// Verifies registration, login, and token generation behaviour.
    /// </summary>
    public class AuthenticationServiceTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly AuthService _authService;

        /// <summary>
        /// Initializes mocks and creates a fresh <see cref="AuthService"/> instance
        /// before each test.
        /// </summary>
        public AuthenticationServiceTests()
        {
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["JWT:Secret"]).Returns("SuperSecretKey12345SuperSecretKey12345");
            _mockConfig.Setup(c => c["JWT:ValidIssuer"]).Returns("AuthBackend");
            _mockConfig.Setup(c => c["JWT:ValidAudience"]).Returns("AuthBackendUsers");

            _authService = new AuthService(_mockUserManager.Object, _mockConfig.Object);
        }

        // RegisterAsync Tests

        /// <summary>
        /// Verifies that <see cref="AuthService.RegisterAsync"/> throws
        /// <see cref="InvalidOperationException"/> when a user with the
        /// same email already exists.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_UserAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingUser = new User { UserName = "test", Email = "test@test.com" };
            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(existingUser);

            var dto = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(dto));
        }

        /// <summary>
        /// Verifies that <see cref="AuthService.RegisterAsync"/> successfully
        /// creates a user with the correct FirstName and LastName
        /// when valid data is provided.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_ValidUser_CreatesUserSuccessfully()
        {
            // Arrange
            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null!);
            _mockUserManager.Setup(u => u.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null!);
            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "password123"
            };

            // Act
            await _authService.RegisterAsync(dto);

            // Assert
            _mockUserManager.Verify(u => u.CreateAsync(
                It.Is<User>(u => u.FirstName == "John" && u.LastName == "Doe"),
                It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Verifies that <see cref="AuthService.RegisterAsync"/> throws
        /// <see cref="InvalidOperationException"/> when Identity fails
        /// to create the user.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_CreateFails_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null!);
            _mockUserManager.Setup(u => u.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null!);
            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            var dto = new RegisterDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@example.com",
                Password = "password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(dto));
        }

        // LoginAsync Tests

        /// <summary>
        /// Verifies that <see cref="AuthService.LoginAsync"/> throws
        /// <see cref="UnauthorizedAccessException"/> when the user
        /// does not exist.
        /// </summary>
        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null!);
            _mockUserManager.Setup(u => u.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null!);

            var dto = new LoginDto
            {
                Email = "nonexistent@test.com",
                Password = "password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(dto));
        }

        /// <summary>
        /// Verifies that <see cref="AuthService.LoginAsync"/> throws
        /// <see cref="UnauthorizedAccessException"/> when the password
        /// is incorrect.
        /// </summary>
        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var user = new User { UserName = "johndoe", Email = "john@example.com" };
            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(false);

            var dto = new LoginDto
            {
                Email = "john@example.com",
                Password = "wrongpassword"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(dto));
        }

        /// <summary>
        /// Verifies that <see cref="AuthService.LoginAsync"/> returns a
        /// non-empty JWT token string when valid credentials are provided.
        /// </summary>
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = "123",
                UserName = "johndoe",
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Doe"
            };
            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(true);

            var dto = new LoginDto
            {
                Email = "john@example.com",
                Password = "password123"
            };

            // Act
            var token = await _authService.LoginAsync(dto);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }
    }
}