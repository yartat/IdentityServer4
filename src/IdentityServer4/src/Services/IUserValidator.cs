using IdentityServer4.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Defines an interface for user profile validator
    /// </summary>
    public interface IUserValidator
    {
        /// <summary>
        /// Determines whether the specified subject is active.
        /// </summary>
        /// <param name="subject">The subject claims.</param>
        /// <param name="client">The client.</param>
        /// <param name="caller">The caller identifier.</param>
        /// <returns>Returns is user specified by subject active.</returns>
        Task<bool> IsActiveAsync(ClaimsPrincipal subject, Client client, string caller);
    }
}
