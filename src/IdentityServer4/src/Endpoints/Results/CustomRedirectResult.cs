// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    /// <summary>
    /// Result for a custom redirect
    /// </summary>
    /// <seealso cref="IdentityServer4.Hosting.IEndpointResult" />
    public class CustomRedirectResult : IEndpointResult
    {
        private readonly ValidatedAuthorizeRequest _request;
        private readonly string _url;
        private IdentityServerOptions _options;
        private ILogger<CustomRedirectResult> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRedirectResult"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="url">The URL.</param>
        /// <exception cref="System.ArgumentNullException">
        /// request
        /// or
        /// url
        /// </exception>
        public CustomRedirectResult(ValidatedAuthorizeRequest request, string url)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            if (url.IsMissing())
            {
                throw new ArgumentNullException(nameof(url));
            }

            _url = url;
        }

        internal CustomRedirectResult(
            ValidatedAuthorizeRequest request,
            string url,
            IdentityServerOptions options)
            : this(request, url)
        {
            _options = options;
        }

        private void Init(HttpContext context)
        {
            _options ??= context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _logger ??= context.RequestServices.GetRequiredService<ILogger<CustomRedirectResult>>();
        }

        /// <summary>
        /// Executes the result.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            _logger.LogTrace("Custom redirect processing");
            var returnUrl = context.GetIdentityServerBasePath().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.Authorize;
            returnUrl = returnUrl.AddQueryString(_request.Raw.ToQueryString());

            if (!_url.IsLocalUrl())
            {
                // this converts the relative redirect path to an absolute one if we're 
                // redirecting to a different server
                _logger.LogTrace("Url is not local. Convert to absolute url.");
                returnUrl = context.GetIdentityServerBaseUri() + returnUrl.RemoveLeadingSlash();
            }

            var url = _url.AddQueryString(_options.UserInteraction.CustomRedirectReturnUrlParameter, returnUrl);
            context.Response.RedirectToAbsoluteUrl(url);

            _logger.LogTrace("Custom redirect completed");
            return Task.CompletedTask;
        }
    }
}