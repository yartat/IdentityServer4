// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    internal class DiscoveryEndpoint : IEndpointHandler
    {
        private static readonly ConcurrentDictionary<int, Dictionary<string, object>> _responseCache = new ConcurrentDictionary<int, Dictionary<string, object>>();

        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IDiscoveryResponseGenerator _responseGenerator;

        public DiscoveryEndpoint(
            IdentityServerOptions options,
            IDiscoveryResponseGenerator responseGenerator,
            ILogger<DiscoveryEndpoint> logger)
        {
            _logger = logger;
            _options = options;
            _responseGenerator = responseGenerator;
        }

        public Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Processing discovery request.");

            // validate HTTP
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                _logger.LogWarning("Discovery endpoint only supports GET requests");
                return Task.FromResult((IEndpointResult) new StatusCodeResult(HttpStatusCode.MethodNotAllowed));
            }

            if (!_options.Endpoints.EnableDiscoveryEndpoint)
            {
                _logger.LogInformation("Discovery endpoint disabled. 404.");
                return Task.FromResult((IEndpointResult) new StatusCodeResult(HttpStatusCode.NotFound));
            }

            var baseUrl = context.GetIdentityServerBaseUri();
            var issuerUri = context.GetIdentityServerIssuerUri();

            // generate response
            _logger.LogTrace("Calling into discovery response generator: {type}", _responseGenerator.GetType().FullName);
            var response = _responseCache.GetOrAdd(1, _ => _responseGenerator.CreateDiscoveryDocumentAsync(baseUrl, issuerUri).GetAwaiter().GetResult());

            _logger.LogTrace("Discovery request completed. Return DiscoveryDocumentResult");
            return Task.FromResult((IEndpointResult) new DiscoveryDocumentResult(response, _options.Discovery.ResponseCacheInterval));
        }
    }
}