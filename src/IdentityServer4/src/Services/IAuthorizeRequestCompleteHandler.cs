using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Defines an interface to handle complete authorize request
    /// </summary>
    public interface IAuthorizeRequestCompleteHandler
    {
        /// <summary>
        /// Processes the complete authorize request asynchronous.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="context">HTTP context instance.</param>
        /// <param name="principal">The claims principal.</param>
        /// <returns>Returns response result.</returns>
        Task<IEndpointResult> ProcessCompleteAuthorizeRequestAsync(NameValueCollection parameters, HttpContext context, ClaimsPrincipal principal = null);
    }
}
