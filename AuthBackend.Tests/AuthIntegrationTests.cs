using System.Net;
using System.Net.Http.Json;
using AuthenticationApi.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AuthenticationApi.Data;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace AuthenticationApi.Tests
{
    /// <summary>
    /// Integration tests for the Authentication API.
    /// Spins up a real in-memory test server and verifies
    /// end-to-end HTTP behaviour for register, login, and user detail endpoints.
    /// </summary>
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        /// <summary>
        /// Sets up the test server with an in-memory database
        /// replacing the real PostgreSQL connection.
        /// </summary>
        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove ALL DbContext registrations
                    var descriptors = services.Where(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                             d.ServiceType == typeof(DbContextOptions) ||
                             d.ServiceType == typeof(AppDbContext)).ToList();

                    foreach (var descriptor in descriptors)
                        services.Remove(descriptor);

                    // Add in-memory database
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

                    // Relax Identity password requirements for tests
                    services.Configure<IdentityOptions>(options =>
                    {
                        options.Password.RequireDigit = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequiredLength = 6;
                    });
                });

                builder.UseSetting("JWT:Secret", "SuperSecretKey12345SuperSecretKey12345");
                builder.UseSetting("JWT:ValidIssuer", "AuthBackend");
                builder.UseSetting("JWT:ValidAudience", "AuthBackendUsers");
            });

            _client = _factory.CreateClient();
        }

        // ─── Register Endpoint Tests ───────────────────────────────────────

        /// <summary>
        /// Verifies that POST /api/auth/register returns 200 OK
        /// when valid registration data is provided.
        /// </summary>
        [Fact]
        public async Task Register_ValidRequest_Returns200()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Verifies that POST /api/auth/register returns 400 Bad Request
        /// when the same user registers twice.
        /// </summary>
        [Fact]
        public async Task Register_DuplicateUser_Returns400()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Username = "janedoe",
                Email = "jane@example.com",
                Password = "password123"
            };

            // Act - register twice
            await _client.PostAsJsonAsync("/api/auth/register", dto);
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Verifies that POST /api/auth/register returns 400 Bad Request
        /// when required fields are missing.
        /// </summary>
        [Fact]
        public async Task Register_MissingFields_Returns400()
        {
            // Arrange - missing required fields
            var dto = new { FirstName = "John", LastName = "Doe" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ─── Login Endpoint Tests ──────────────────────────────────────────

        /// <summary>
        /// Verifies that POST /api/auth/login returns 200 OK with a token
        /// when valid credentials are provided.
        /// </summary>
        [Fact]
        public async Task Login_ValidCredentials_Returns200WithToken()
        {
            // Arrange - first register a user
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Smith",
                Username = "johnsmith",
                Email = "johnsmith@example.com",
                Password = "password123"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto
            {
                Username = "johnsmith",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.True(body.ContainsKey("accessToken"));
            Assert.NotEmpty(body["accessToken"]);
        }

        /// <summary>
        /// Verifies that POST /api/auth/login returns 401 Unauthorized
        /// when invalid credentials are provided.
        /// </summary>
        [Fact]
        public async Task Login_InvalidCredentials_Returns401()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "nonexistent",
                Password = "wrongpassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ─── Me Endpoint Tests ─────────────────────────────────────────────

        /// <summary>
        /// Verifies that GET /api/auth/me returns 401 Unauthorized
        /// when no JWT token is provided.
        /// </summary>
        [Fact]
        public async Task GetCurrentUser_NoToken_Returns401()
        {
            // Act
            var response = await _client.GetAsync("/api/auth/me");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Verifies that GET /api/auth/me returns 200 OK with user details
        /// when a valid JWT token is provided.
        /// </summary>
        [Fact]
        public async Task GetCurrentUser_ValidToken_Returns200WithUserDetails()
        {
            // Arrange - register and login to get token
            var registerDto = new RegisterDto
            {
                FirstName = "Alice",
                LastName = "Smith",
                Username = "alicesmith",
                Email = "alice@example.com",
                Password = "password123"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto
            {
                Username = "alicesmith",
                Password = "password123"
            };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var token = loginBody!["accessToken"];

            // Act - call /me with token
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("/api/auth/me");
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal("alice@example.com", body["email"]);
            Assert.Equal("Alice", body["firstName"]);
            Assert.Equal("Smith", body["lastName"]);
        }
    }
}
