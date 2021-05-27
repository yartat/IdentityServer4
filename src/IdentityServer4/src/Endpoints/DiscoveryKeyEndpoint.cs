// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    internal class DiscoveryKeyEndpoint : IEndpointHandler
    {
        private static readonly ConcurrentDictionary<int, IEnumerable<JsonWebKey>> _responseCache = new ConcurrentDictionary<int, IEnumerable<JsonWebKey>>();

        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IDiscoveryResponseGenerator _responseGenerator;

        public DiscoveryKeyEndpoint(
            IdentityServerOptions options,
            IDiscoveryResponseGenerator responseGenerator,
            ILogger<DiscoveryKeyEndpoint> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _responseGenerator = responseGenerator ?? throw new ArgumentNullException(nameof(responseGenerator));
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

            if (!_options.Discovery.ShowKeySet)
            {
                _logger.LogInformation("Key discovery disabled. 404.");
                return Task.FromResult((IEndpointResult) new StatusCodeResult(HttpStatusCode.NotFound));
            }

            // generate response
            _logger.LogTrace("Calling into discovery response generator: {type}", _responseGenerator.GetType().FullName);
            var response = _responseCache.GetOrAdd(1, _ => _responseGenerator.CreateJwkDocumentAsync().GetAwaiter().GetResult());

            _logger.LogTrace("Discovery request completed. Return JsonWebKeysResult");
            return Task.FromResult((IEndpointResult) new JsonWebKeysResult(response, _options.Discovery.ResponseCacheInterval));
        }
    }
}
