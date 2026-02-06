// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
#if NET7_0_OR_GREATER
#else
using Microsoft.AspNetCore.Authentication;
#endif
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    /// <summary>
    /// Defines an authorize result response.
    /// Implements the <see cref="IEndpointResult" />
    /// </summary>
    /// <seealso cref="IEndpointResult" />
    /// <remarks>
    /// Initializes a new instance of the <see cref="AuthorizeResult"/> class.
    /// </remarks>
    /// <param name="response">The response.</param>
    /// <exception cref="ArgumentNullException">response</exception>
    public class AuthorizeResult(AuthorizeResponse response) : IEndpointResult
    {
        private const string FormPostHtml = "<html><head><meta http-equiv='X-UA-Compatible' content='IE=edge' /><base target='_self'/></head><body><form method='post' action='{uri}'>{body}<noscript><button>Click to continue</button></noscript></form><script>window.addEventListener('load', function(){document.forms[0].submit();});</script></body></html>";

        private IdentityServerOptions? _options;
        private IUserSession? _userSession;
        private IMessageStore<ErrorMessage>? _errorMessageStore;
#if NET7_0_OR_GREATER
        private TimeProvider? _clock;
#else
        private ISystemClock? _clock;
#endif
        private ILogger<AuthorizeResult>? _logger;

        internal AuthorizeResult(
            AuthorizeResponse response,
            IdentityServerOptions options,
            IUserSession userSession,
            IMessageStore<ErrorMessage> errorMessageStore,
#if NET7_0_OR_GREATER
            TimeProvider clock)
#else
            ISystemClock clock)
#endif
            : this(response)
        {
            _options = options;
            _userSession = userSession;
            _errorMessageStore = errorMessageStore;
            _clock = clock;
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <value>The response.</value>
        public AuthorizeResponse Response { get; } = response ?? throw new ArgumentNullException(nameof(response));

        /// <inheritdoc/>
        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            _logger.LogTrace("Authorize result executing");
            if (Response.IsError)
            {
                _logger.LogTrace("Authorize result is error");
                await ProcessErrorAsync(context);
            }
            else
            {
                await ProcessResponseAsync(context);
            }

            _logger.LogTrace("Authorize result completed");
        }

        [MemberNotNull(nameof(_options), nameof(_userSession), nameof(_errorMessageStore), nameof(_clock), nameof(_logger))]
        private void Init(HttpContext context)
        {
            _options ??= context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _userSession ??= context.RequestServices.GetRequiredService<IUserSession>();
            _errorMessageStore ??= context.RequestServices.GetRequiredService<IMessageStore<ErrorMessage>>();
#if NET7_0_OR_GREATER
            _clock ??= context.RequestServices.GetRequiredService<TimeProvider>();
#else
            _clock ??= context.RequestServices.GetRequiredService<ISystemClock>();
#endif
            _logger ??= context.RequestServices.GetRequiredService<ILogger<AuthorizeResult>>();
        }

        private async Task ProcessErrorAsync(HttpContext context)
        {
            // these are the conditions where we can send a response 
            // back directly to the client, otherwise we're only showing the error UI
            var isSafeError =
                Response.Error == OidcConstants.AuthorizeErrors.AccessDenied ||
                Response.Error == OidcConstants.AuthorizeErrors.AccountSelectionRequired ||
                Response.Error == OidcConstants.AuthorizeErrors.LoginRequired ||
                Response.Error == OidcConstants.AuthorizeErrors.ConsentRequired ||
                Response.Error == OidcConstants.AuthorizeErrors.InteractionRequired;

            if (isSafeError)
            {
                // this scenario we can return back to the client
                _logger!.LogTrace("Safe error");
                await ProcessResponseAsync(context);
            }
            else
            {
                // we now know we must show error page
                _logger!.LogTrace("Redirect to error page");
                await RedirectToErrorPageAsync(context);
            }
        }

        /// <summary>
        /// Processes the response asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        protected async Task ProcessResponseAsync(HttpContext context)
        {
            if (!Response.IsError)
            {
                // success response -- track client authorization for sign-out
                _logger!.LogTrace("Adding client {0} to client list cookie for subject {1}", Response.Request?.ClientId, Response.Request?.Subject.GetSubjectId());
                await _userSession!.AddClientIdAsync(Response.Request?.ClientId);
            }

            await RenderAuthorizeResponseAsync(context);
        }

        private async Task RenderAuthorizeResponseAsync(HttpContext context)
        {
            _logger!.LogTrace("Render authorize response");
            if (Response.Request?.ResponseMode == OidcConstants.ResponseModes.Query ||
                Response.Request?.ResponseMode == OidcConstants.ResponseModes.Fragment)
            {
                _logger!.LogTrace("Render as redirect");
                context.Response.SetNoCache();
                context.Response.Redirect(BuildRedirectUri());
            }
            else if (Response.Request?.ResponseMode == OidcConstants.ResponseModes.FormPost)
            {
                _logger!.LogTrace("Render as form POST");
                context.Response.SetNoCache();
                AddSecurityHeaders(context);
                await context.Response.WriteHtmlAsync(GetFormPostHtml());
            }
            else
            {
                _logger!.LogWarning("Unsupported response mode.");
                throw new InvalidOperationException("Unsupported response mode");
            }
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            context.Response.AddScriptCspHeaders(_options!.Csp, "sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8=");

            const string referrer_policy = "no-referrer";
            context.Response.Headers["Referrer-Policy"] = referrer_policy;
        }

        private string BuildRedirectUri()
        {
            var uri = Response.RedirectUri;
            var query = Response.ToNameValueCollection().ToQueryString();

            if (Response.Request?.ResponseMode == OidcConstants.ResponseModes.Query)
            {
                uri = uri.AddQueryString(query);
            }
            else
            {
                uri = uri.AddHashFragment(query);
            }

            if (Response.IsError && !uri.Contains("#"))
            {
                // https://tools.ietf.org/html/draft-bradley-oauth-open-redirector-00
                uri += "#_=_";
            }

            return uri;
        }

        private string GetFormPostHtml()
        {
            var html = FormPostHtml;

            var url = Response.Request?.RedirectUri;
            url = HtmlEncoder.Default.Encode(url ?? string.Empty);
            html = html.Replace("{uri}", url);
            html = html.Replace("{body}", Response.ToNameValueCollection().ToFormPost());

            return html;
        }

        private async Task RedirectToErrorPageAsync(HttpContext context)
        {
            var errorModel = new ErrorMessage
            {
                RequestId = context.TraceIdentifier,
                Error = Response.Error,
                ErrorDescription = Response.ErrorDescription,
                UiLocales = Response.Request?.UiLocales,
                DisplayMode = Response.Request?.DisplayMode,
                ClientId = Response.Request?.ClientId
            };

            if (Response.RedirectUri != null && Response.Request?.ResponseMode != null)
            {
                // if we have a valid redirect uri, then include it to the error page
                errorModel.RedirectUri = BuildRedirectUri();
                errorModel.ResponseMode = Response.Request.ResponseMode;
            }

#if NET7_0_OR_GREATER
            var message = new Message<ErrorMessage>(errorModel, _clock!.GetUtcNow().UtcDateTime);
#else
            var message = new Message<ErrorMessage>(errorModel, _clock!.UtcNow.UtcDateTime);
#endif
            var id = await _errorMessageStore!.WriteAsync(message);

            var errorUrl = _options!.UserInteraction.ErrorUrl;

            var url = errorUrl.AddQueryString(_options.UserInteraction.ErrorIdParameter, id);
            context.Response.RedirectToAbsoluteUrl(url);
        }
    }
}
