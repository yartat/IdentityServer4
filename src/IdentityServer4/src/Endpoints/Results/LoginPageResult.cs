// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using IdentityServer4.Hosting;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Extensions;
using IdentityServer4.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Services;

namespace IdentityServer4.Endpoints.Results
{
    /// <summary>
    /// Result for login page
    /// </summary>
    /// <seealso cref="IdentityServer4.Hosting.IEndpointResult" />
    public class LoginPageResult : IEndpointResult
    {
        private readonly ValidatedAuthorizeRequest _request;

        private IdentityServerOptions _options;
        private IAuthorizationParametersProcessor _authorizationParametersProcessor;
        private readonly ILoginUrlProcessor _loginUrlProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPageResult"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="loginUrlProcessor">The login URL processor instance.</param>
        /// <exception cref="ArgumentNullException">request</exception>
        public LoginPageResult(
            ValidatedAuthorizeRequest request,
            ILoginUrlProcessor loginUrlProcessor = null)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _loginUrlProcessor = loginUrlProcessor;
        }

        internal LoginPageResult(
            ValidatedAuthorizeRequest request,
            IdentityServerOptions options,
            IAuthorizationParametersProcessor authorizationParametersProcessor = null,
            ILoginUrlProcessor loginUrlProcessor = null)
            : this(request, loginUrlProcessor)
        {
            _options = options;
            _authorizationParametersProcessor = authorizationParametersProcessor;
        }

        private void Init(HttpContext context)
        {
            _options ??= context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _authorizationParametersProcessor ??= context.RequestServices.GetService<IAuthorizationParametersProcessor>();
        }

        /// <summary>
        /// Executes the result.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            var (returnUrl, otherParameters) = await _authorizationParametersProcessor.StoreParametersAsync(_request);
            var loginUrl = _options.UserInteraction.LoginUrl;
            var resultUrl = loginUrl;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = context.GetIdentityServerBasePath().EnsureTrailingSlash() + returnUrl;
                if (!loginUrl.IsLocalUrl())
                {
                    // this converts the relative redirect path to an absolute one if we're 
                    // redirecting to a different server
                    returnUrl = _options.BaseUri.EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
                }

                if (!string.IsNullOrEmpty(otherParameters))
                {
                    returnUrl += otherParameters;
                }

                resultUrl = loginUrl.AddQueryString(_options.UserInteraction.LoginReturnUrlParameter, returnUrl);
            }

            if (_loginUrlProcessor != null)
            {
                resultUrl = _loginUrlProcessor.Process(resultUrl, _request.Raw.ToFullDictionary());
            }

            context.Response.RedirectToAbsoluteUrl(resultUrl);
        }
    }
}