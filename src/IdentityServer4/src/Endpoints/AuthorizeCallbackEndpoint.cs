// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    /// <summary>
    /// Defines authorize callback endpoint.
    /// Implements the <see cref="IEndpointHandler" />
    /// </summary>
    /// <seealso cref="IEndpointHandler" />
    internal class AuthorizeCallbackEndpoint : IEndpointHandler
    {
        private readonly IAuthorizeRequestCompleteHandler _requestHandler;
        private readonly ILogger<AuthorizeCallbackEndpoint> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeCallbackEndpoint"/> class.
        /// </summary>
        /// <param name="requestHandler">The request handler.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">requestHandler</exception>
        /// <exception cref="ArgumentNullException">logger</exception>
        public AuthorizeCallbackEndpoint(
            IAuthorizeRequestCompleteHandler requestHandler,
            ILogger<AuthorizeCallbackEndpoint> logger)
        {
            _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                _logger.LogWarning("Invalid HTTP method for authorize endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            _logger.LogTrace("Start authorize callback request");
            var parameters = context.Request.Query.AsNameValueCollection();
            var result = await _requestHandler.ProcessCompleteAuthorizeRequestAsync(parameters, context);

            _logger.LogTrace("Authorize callback request completed");
            return result;
        }
    }
}
