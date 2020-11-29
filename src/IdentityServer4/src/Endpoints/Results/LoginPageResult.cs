// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Hosting;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Extensions;
using IdentityServer4.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Stores;
using IdentityServer4.Models;
using System.Collections.Specialized;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPageResult"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <exception cref="System.ArgumentNullException">request</exception>
        public LoginPageResult(ValidatedAuthorizeRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        internal LoginPageResult(
            ValidatedAuthorizeRequest request,
            IdentityServerOptions options,
            IAuthorizationParametersProcessor authorizationParametersProcessor = null) 
            : this(request)
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
        /// <returns></returns>
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

            context.Response.RedirectToAbsoluteUrl(resultUrl);
        }
    }
}