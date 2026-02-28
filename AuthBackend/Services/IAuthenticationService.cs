using AuthenticationApi.DTOs;

namespace AuthenticationApi.Services
{
    /// <summary>
    /// Defines authentication-related operations such as
    /// user registration and login.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">The user registration details.</param>
        /// <remarks>
        /// This method validates the input, hashes the user's password,
        /// and persists the user to the database.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a user with the same email already exists.
        /// </exception>
        Task RegisterAsync(RegisterDto request);

        /// <summary>
        /// Authenticates a user and generates a JWT access token.
        /// </summary>
        /// <param name="request">The user login credentials.</param>
        /// <returns>
        /// A JWT token string if authentication is successful.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when the provided credentials are invalid.
        /// </exception>
        Task<string> LoginAsync(LoginDto request);
    }
}