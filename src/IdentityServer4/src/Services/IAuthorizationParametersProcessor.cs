using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Defines an interface for authorization parameters processor
    /// </summary>
    public interface IAuthorizationParametersProcessor
    {
        /// <summary>
        /// Stores the parameters asynchronous.
        /// </summary>
        /// <param name="request">The authorize request.</param>
        /// <returns>Returns identifier of the stored authorized parameters</returns>
        Task<(string ReturnUrl, string OtherParameters)> StoreParametersAsync(ValidatedAuthorizeRequest request);
    }
}
