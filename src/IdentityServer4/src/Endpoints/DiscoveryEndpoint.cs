// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    internal class DiscoveryEndpoint : IEndpointHandler
    {
        private const string CacheKeyPrefix = "IdentityServer4:DiscoveryDocument:";

        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IDiscoveryResponseGenerator _responseGenerator;
        private readonly IMemoryCache _cache;

        public DiscoveryEndpoint(
            IdentityServerOptions options,
            IDiscoveryResponseGenerator responseGenerator,
            IMemoryCache cache,
            ILogger<DiscoveryEndpoint> logger)
        {
            _logger = logger;
            _options = options;
            _responseGenerator = responseGenerator;
            _cache = cache;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Processing discovery request.");

            // validate HTTP
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                _logger.LogWarning("Discovery endpoint only supports GET requests");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            if (!_options.Endpoints.EnableDiscoveryEndpoint)
            {
                _logger.LogInformation("Discovery endpoint disabled. 404.");
                return new StatusCodeResult(HttpStatusCode.NotFound);
            }

            var baseUrl = context.GetIdentityServerBaseUri();
            var issuerUri = context.GetIdentityServerIssuerUri();

            // generate response; the document embeds both URLs, so each combination is cached separately
            _logger.LogTrace("Calling into discovery response generator: {type}", _responseGenerator.GetType().FullName);
            var response = await _cache.GetOrCreateWithExpirationAsync(
                CacheKeyPrefix + issuerUri + "|" + baseUrl,
                _options.Discovery.CacheDuration,
                () => _responseGenerator.CreateDiscoveryDocumentAsync(baseUrl, issuerUri));

            _logger.LogTrace("Discovery request completed. Return DiscoveryDocumentResult");
            return new DiscoveryDocumentResult(response, _options.Discovery.ResponseCacheInterval);
        }
    }
}
