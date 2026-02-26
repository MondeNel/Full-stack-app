using AuthenticationApi.DTOs;

namespace AuthenticationApi.Services
{
    /// <summary>
    /// Authentication service contract
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">User registration data</param>
        Task RegisterAsync(RegisterDto request);

        /// <summary>
        /// Authenticates user and returns JWT token
        /// </summary>
        /// <param name="request">User login credentials</param>
        /// <returns>JWT token</returns>
        Task<string> LoginAsync(LoginDto request);
    }
}