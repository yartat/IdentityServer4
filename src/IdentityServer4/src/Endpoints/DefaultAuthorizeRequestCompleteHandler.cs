using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    /// <summary>
    /// Defines a default handler for the complete authorize request.
    /// Implements the <see cref="IAuthorizeRequestCompleteHandler" />
    /// </summary>
    /// <seealso cref="IAuthorizeRequestCompleteHandler" />
    public class DefaultAuthorizeRequestCompleteHandler : IAuthorizeRequestCompleteHandler
    {
        private readonly IAuthorizeRequestHandler _authorizeRequestHandler;
        private readonly IConsentMessageStore _consentResponseStore;
        private readonly IAuthorizationParametersMessageStore _authorizationParametersMessageStore;
        private readonly ILogger<DefaultAuthorizeRequestCompleteHandler> _logger;
        private readonly IUserSession _userSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAuthorizeRequestCompleteHandler"/> class.
        /// </summary>
        /// <param name="authorizeRequestHandler">The authorize request handler.</param>
        /// <param name="consentResponseStore">The consent response store instance.</param>
        /// <param name="authorizationParametersMessageStore">The authorization parameters message store instance.</param>
        /// <param name="userSession">The user session instance.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">authorizeRequestHandler</exception>
        /// <exception cref="ArgumentNullException">consentResponseStore</exception>
        /// <exception cref="ArgumentNullException">logger</exception>
        /// <exception cref="ArgumentNullException">userSession</exception>
        public DefaultAuthorizeRequestCompleteHandler(
            IAuthorizeRequestHandler authorizeRequestHandler,
            IConsentMessageStore consentResponseStore,
            IUserSession userSession,
            ILogger<DefaultAuthorizeRequestCompleteHandler> logger,
            IAuthorizationParametersMessageStore authorizationParametersMessageStore = null)
        {
            _authorizeRequestHandler = authorizeRequestHandler ?? throw new ArgumentNullException(nameof(authorizeRequestHandler));
            _consentResponseStore = consentResponseStore ?? throw new ArgumentNullException(nameof(consentResponseStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
            _authorizationParametersMessageStore = authorizationParametersMessageStore;
        }

        /// <inheritdoc/>
        public async Task<IEndpointResult> ProcessCompleteAuthorizeRequestAsync(
            NameValueCollection parameters,
            HttpContext context,
            ClaimsPrincipal principal = null)
        {
            _logger.LogTrace("Processing complete authorize request");
            if (_authorizationParametersMessageStore != null)
            {
                _logger.LogTrace("Check parameters from message store");
                var messageStoreId = parameters[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
                var entry = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);
                parameters = entry?.Data.FromFullDictionary() ?? new NameValueCollection();

                _logger.LogTrace("Remove parameters from message store");
                await _authorizationParametersMessageStore.DeleteAsync(messageStoreId);
            }

            _logger.LogTrace("Get user session");
            var user = principal ?? await _userSession.GetUserAsync();
            var consentRequest = new ConsentRequest(parameters, user?.GetSubjectId());
            var consent = await _consentResponseStore.ReadAsync(consentRequest.Id);

            if (consent != null && consent.Data == null)
            {
                _logger.LogWarning("Consent message is missing data");
                return await _authorizeRequestHandler.CreateErrorResultAsync("consent message is missing data");
            }

            try
            {
                _logger.LogTrace("Authorize Request processing");
                var result = await _authorizeRequestHandler.ProcessAuthorizeRequestAsync(parameters, user, consent?.Data, context);
                _logger.LogTrace("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");
                return result;
            }
            finally
            {
                if (consent != null)
                {
                    await _consentResponseStore.DeleteAsync(consentRequest.Id);
                }

                _logger.LogTrace("Complete authorize request completed.");
            }
        }
    }
}
