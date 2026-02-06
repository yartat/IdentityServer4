// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    internal class AuthorizeEndpoint : IEndpointHandler
    {
        private readonly IAuthorizeRequestHandler _requestHandler;
        private readonly ILogger<AuthorizeEndpoint> _logger;
        private readonly IUserSession _userSession;

        public AuthorizeEndpoint(
           IAuthorizeRequestHandler requestHandler,
           ILogger<AuthorizeEndpoint> logger,
           IUserSession userSession)
        {
            _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
        }

        public async Task<IEndpointResult?> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Start authorize request");
            NameValueCollection values;

            if (HttpMethods.IsGet(context.Request.Method))
            {
                values = context.Request.Query.AsNameValueCollection();
            }
            else if (HttpMethods.IsPost(context.Request.Method))
            {
                if (!context.Request.HasFormContentType)
                {
                    _logger.LogWarning("Unsupported content type for POST authorize request");
                    return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
                }

                values = context.Request.Form.AsNameValueCollection();
            }
            else
            {
                _logger.LogWarning("Authorize method not allowed");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            _logger.LogTrace("Getting user session");
            var user = await _userSession.GetUserAsync(true);

            _logger.LogTrace("Processing authorize");
            var result = await _requestHandler.ProcessAuthorizeRequestAsync(values, user, null, context);

            _logger.LogTrace("End authorize request. result type: {0}", result?.GetType().ToString() ?? "-none-");
            return result;
        }
    }
}
