using System.Net;
using System.Net.Http.Json;
using AuthenticationApi.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AuthenticationApi.Data;
using AuthenticationApi.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Json;
using System.Net;
using Xunit;

namespace AuthenticationApi.Tests
{
    /// <summary>
    /// Custom WebApplicationFactory that sets the environment to Testing
    /// so Program.cs uses the in-memory database instead of PostgreSQL.
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // This tells Program.cs to use InMemory database
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
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
        }
    }

    /// <summary>
    /// Integration tests for the Authentication API.
    /// </summary>
    public class AuthIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public AuthIntegrationTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        // ─── Register Endpoint Tests ───────────────────────────────────────

        /// <summary>
        /// Verifies that POST /api/auth/register returns 200 OK
        /// when valid registration data is provided.
        /// </summary>
        [Fact]
        public async Task Register_ValidRequest_Returns200()
        {
            var dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = $"john_{Guid.NewGuid()}@example.com",
                Password = "password123"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Verifies that POST /api/auth/register returns 400 Bad Request
        /// when the same user registers twice.
        /// </summary>
        [Fact]
        public async Task Register_DuplicateUser_Returns400()
        {
            var dto = new RegisterDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = $"jane_{Guid.NewGuid()}@example.com",
                Password = "password123"
            };

            await _client.PostAsJsonAsync("/api/auth/register", dto);
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Verifies that POST /api/auth/register returns 400 Bad Request
        /// when required fields are missing.
        /// </summary>
        [Fact]
        public async Task Register_MissingFields_Returns400()
        {
            var dto = new { FirstName = "John", LastName = "Doe" };

            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

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
            var username = "johnsmith_" + Guid.NewGuid();
            var email = $"johnsmith_{Guid.NewGuid()}@example.com";

            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Smith",
                Email = email,
                Password = "password123"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto
            {
                Email = email,
                Password = "password123"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

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
            var loginDto = new LoginDto
            {
                Email = "nonexistent_" + Guid.NewGuid() + "@example.com",
                Password = "wrongpassword"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

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
            var response = await _client.GetAsync("/api/auth/me");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Verifies that GET /api/auth/me returns 200 OK with user details
        /// when a valid JWT token is provided.
        /// </summary>
        [Fact]
        public async Task GetCurrentUser_ValidToken_Returns200WithUserDetails()
        {
            var username = "alicesmith_" + Guid.NewGuid();
            var email = $"alice_{Guid.NewGuid()}@example.com";

            var registerDto = new RegisterDto
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = email,
                Password = "password123"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto { Email = email, Password = "password123" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var token = loginBody!["accessToken"];

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("/api/auth/me");
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal(email, body["email"]);
            Assert.Equal("Alice", body["firstName"]);
            Assert.Equal("Smith", body["lastName"]);
        }
    }
}