using IdentityModel;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Defines an interface to handle authorize request
    /// </summary>
    public interface IAuthorizeRequestHandler
    {
        /// <summary>
        /// Processes the authorize request asynchronous.
        /// </summary>
        /// <param name="parameters">The authorization parameters.</param>
        /// <param name="user">The user principal.</param>
        /// <param name="consent">The consent response.</param>
        /// <param name="context">The HTTP context instance.</param>
        /// <returns>Returns response.</returns>
        Task<IEndpointResult> ProcessAuthorizeRequestAsync(NameValueCollection parameters, ClaimsPrincipal user, ConsentResponse consent, HttpContext context);

        /// <summary>
        /// Creates the error result asynchronous.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="request">The request instance.</param>
        /// <param name="error">The error field.</param>
        /// <param name="errorDescription">The error description.</param>
        /// <param name="logError">if set to <c>true</c> log error.</param>
        /// <returns>Returns response.</returns>
        Task<IEndpointResult> CreateErrorResultAsync(
            string logMessage,
            ValidatedAuthorizeRequest request = null,
            string error = OidcConstants.AuthorizeErrors.ServerError,
            string errorDescription = null,
            bool logError = true);
    }
}
