using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Models a user's authentication session
    /// </summary>
    public interface IUserSession
    {
        /// <summary>
        /// Creates a session identifier for the signin context and issues the session id cookie.
        /// </summary>
        Task<string> CreateSessionIdAsync(ClaimsPrincipal principal, AuthenticationProperties properties);

        /// <summary>
        /// Gets the current authenticated user.
        /// </summary>
        /// <param name="recheck">Indicates should recheck session.</param>
        /// <returns>Returns session user claims</returns>
        Task<ClaimsPrincipal> GetUserAsync(bool recheck = false);

        /// <summary>
        /// Gets the current session identifier.
        /// </summary>
        /// <param name="recheck">Indicates should recheck session.</param>
        /// <returns>Returns session id</returns>
        Task<string> GetSessionIdAsync(bool recheck = false);

        /// <summary>
        /// Ensures the session identifier cookie asynchronous.
        /// </summary>
        /// <returns></returns>
        Task EnsureSessionIdCookieAsync();

        /// <summary>
        /// Removes the session identifier cookie.
        /// </summary>
        Task RemoveSessionIdCookieAsync();

        /// <summary>
        /// Adds a client to the list of clients the user has signed into during their session.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        Task AddClientIdAsync(string clientId);

        /// <summary>
        /// Gets the list of clients the user has signed into during their session.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetClientListAsync();
    }
}
