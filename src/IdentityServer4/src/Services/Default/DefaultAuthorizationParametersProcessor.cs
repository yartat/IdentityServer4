using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services.Default
{
    /// <summary>
    /// Defines a default authorization parameters processor.
    /// Implements the <see cref="IAuthorizationParametersProcessor" />
    /// </summary>
    /// <seealso cref="IAuthorizationParametersProcessor" />
    public class DefaultAuthorizationParametersProcessor : IAuthorizationParametersProcessor
    {
        private readonly IAuthorizationParametersMessageStore _store;
        private readonly ILogger<DefaultAuthorizationParametersProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAuthorizationParametersProcessor"/> class.
        /// </summary>
        /// <param name="store">The authorization parameters store instance.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">logger</exception>
        public DefaultAuthorizationParametersProcessor(
            IAuthorizationParametersMessageStore store,
            ILogger<DefaultAuthorizationParametersProcessor> logger)
        {
            _store = store;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<(string ReturnUrl, string OtherParameters)> StoreParametersAsync(ValidatedAuthorizeRequest request)
        {
            var otherParameters = string.Empty;
            if (_store != null)
            {
                var msg = new Message<IDictionary<string, string[]>>(request.Raw.ToFullDictionary());
                var id = await _store.WriteAsync(msg);
                otherParameters = otherParameters.AddQueryString(Constants.AuthorizationParamsStore.MessageStoreIdParameterName, id);
            }
            else
            {
                otherParameters = otherParameters.AddQueryString(request.Raw.ToQueryString());
            }

            return (Constants.ProtocolRoutePaths.AuthorizeCallback, otherParameters);
        }
    }
}
