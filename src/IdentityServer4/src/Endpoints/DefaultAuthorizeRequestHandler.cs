using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Logging.Models;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    /// <summary>
    /// Defines a default handler implementation for the authorize request.
    /// Implements the <see cref="IAuthorizeRequestHandler" />
    /// </summary>
    /// <seealso cref="IAuthorizeRequestHandler" />
    public class DefaultAuthorizeRequestHandler: IAuthorizeRequestHandler
    {
        private readonly IAuthorizeResponseGenerator _authorizeResponseGenerator;
        private readonly IEventService _events;
        private readonly IAuthorizeInteractionResponseGenerator _interactionGenerator;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly ILoginUrlProcessor? _loginUrlProcessor;
        private readonly IUserSession _userSession;
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAuthorizeRequestHandler"/> class.
        /// </summary>
        /// <param name="events">The events service instance.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="validator">The request validator instance.</param>
        /// <param name="interactionGenerator">The interaction generator instance.</param>
        /// <param name="authorizeResponseGenerator">The authorize response generator instance.</param>
        /// <param name="userSession">The user session instance.</param>
        /// <param name="options">IdentityServer options.</param>
        /// <param name="loginUrlProcessor">The login URL processor instance.</param>
        /// <exception cref="ArgumentNullException">events</exception>
        /// <exception cref="ArgumentNullException">logger</exception>
        /// <exception cref="ArgumentNullException">validator</exception>
        /// <exception cref="ArgumentNullException">interactionGenerator</exception>
        /// <exception cref="ArgumentNullException">authorizeResponseGenerator</exception>
        /// <exception cref="ArgumentNullException">userSession</exception>
        public DefaultAuthorizeRequestHandler(
            IEventService events,
            ILogger<DefaultAuthorizeRequestHandler> logger,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeResponseGenerator authorizeResponseGenerator,
            IUserSession userSession,
            IOptions<IdentityServerOptions> options,
            ILoginUrlProcessor? loginUrlProcessor = null)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _interactionGenerator = interactionGenerator ?? throw new ArgumentNullException(nameof(interactionGenerator));
            _authorizeResponseGenerator = authorizeResponseGenerator ?? throw new ArgumentNullException(nameof(authorizeResponseGenerator));
            _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _loginUrlProcessor = loginUrlProcessor;
        }

        /// <inheritdoc/>
        public async Task<IEndpointResult> ProcessAuthorizeRequestAsync(
            NameValueCollection parameters,
            ClaimsPrincipal user,
            ConsentResponse? consent,
            HttpContext context)
        {
            if (user != null)
            {
                _logger.LogTrace("User in authorize request: {subjectId}", user.GetSubjectId());
            }
            else
            {
                _logger.LogTrace("No user present in authorize request");
            }

            // validate request
            _logger.LogTrace("Validates request");
            var result = await _validator.ValidateAsync(parameters, user);
            if (result.IsError)
            {
                return await CreateErrorResultAsync(
                    "Request validation failed",
                    result.ValidatedRequest,
                    result.Error,
                    result.ErrorDescription);
            }

            var request = result.ValidatedRequest;
            LogRequest(request);

            // determine user interaction
            _logger.LogTrace("Process interaction request");
            var interactionResult = await _interactionGenerator.ProcessInteractionAsync(request, consent);
            if (interactionResult.IsError)
            {
                _logger.LogDebug("Iteration result is error");
                return await CreateErrorResultAsync("Interaction generator error", request, interactionResult.Error, interactionResult.ErrorDescription, false);
            }
            if (interactionResult.IsLogin)
            {
                _logger.LogTrace("Iteration result is login");
                return new LoginPageResult(request, _loginUrlProcessor);
            }
            if (interactionResult.IsConsent)
            {
                _logger.LogTrace("Iteration result is consent");
                return new ConsentPageResult(request);
            }
            if (interactionResult.IsRedirect)
            {
                _logger.LogTrace("Iteration result is redirect");
                return new CustomRedirectResult(request, interactionResult.RedirectUrl);
            }

            _logger.LogTrace("Enrich by IP and device type");
            request.ClientIp = context.GetRequestIp();
            request.Device = context
                .GetHeaderValueAs<string>("User-Agent")
                .GetDevice();

            _logger.LogTrace("Generate response");
            var response = await _authorizeResponseGenerator.CreateResponseAsync(request);
            await RaiseResponseEventAsync(response);
            LogResponse(response);

            _logger.LogTrace("Response completed as AuthorizeResult");
            return new AuthorizeResult(response);
        }

        /// <inheritdoc/>
        public async Task<IEndpointResult> CreateErrorResultAsync(
            string logMessage,
            ValidatedAuthorizeRequest? request = null,
            string error = OidcConstants.AuthorizeErrors.ServerError,
            string? errorDescription = null,
            bool logError = true)
        {
            if (logError)
            {
                _logger.LogError(logMessage);
            }

            if (request != null)
            {
                var details = new AuthorizeRequestValidationLog(request, _options.Logging.AuthorizeRequestSensitiveValuesFilter);
                _logger.LogInformation("{@validationDetails}", details);
            }

            // TODO: should we raise a token failure event for all errors to the authorize endpoint?
            await RaiseFailureEventAsync(request, error, errorDescription);

            return new AuthorizeResult(new AuthorizeResponse
            {
                Request = request,
                Error = error,
                ErrorDescription = errorDescription,
                SessionState = request?.GenerateSessionStateValue()
            });
        }

        private void LogRequest(ValidatedAuthorizeRequest request)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                var details = new AuthorizeRequestValidationLog(request, _options.Logging.AuthorizeRequestSensitiveValuesFilter);
                _logger.LogTrace(nameof(ValidatedAuthorizeRequest) + Environment.NewLine + "{@validationDetails}", details);
            }
        }

        private void LogResponse(AuthorizeResponse response)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                var details = new AuthorizeResponseLog(response);
                _logger.LogTrace("Authorize endpoint response" + Environment.NewLine + "{@details}", details);
            }
        }

        private void LogTokens(AuthorizeResponse response)
        {
            if (!_logger.IsEnabled(LogLevel.Trace))
            {
                return;
            }

            var clientId = $"{response.Request?.ClientId} ({response.Request?.Client.ClientName ?? "no name set"})";
            var subjectId = response.Request?.Subject.GetSubjectId();

            if (response.IdentityToken != null)
            {
                _logger.LogTrace("Identity token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.IdentityToken);
            }
            if (response.Code != null)
            {
                _logger.LogTrace("Code issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.Code);
            }
            if (response.AccessToken != null)
            {
                _logger.LogTrace("Access token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.AccessToken);
            }
        }

        private Task RaiseFailureEventAsync(ValidatedAuthorizeRequest? request, string error, string? errorDescription) =>
            _events.RaiseAsync(new TokenIssuedFailureEvent(request, error, errorDescription));

        private Task RaiseResponseEventAsync(AuthorizeResponse response)
        {
            if (!response.IsError)
            {
                LogTokens(response);
                return _events.RaiseAsync(new TokenIssuedSuccessEvent(response));
            }
            else
            {
                return RaiseFailureEventAsync(response.Request, response.Error, response.ErrorDescription);
            }
        }
    }
}
