// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer4.Hosting
{
    public class BaseUrlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IdentityServerOptions _options;
        private readonly ILogger<BaseUrlMiddleware> _logger;

        public BaseUrlMiddleware(RequestDelegate next, IdentityServerOptions options, ILogger<BaseUrlMiddleware> logger)
        {
            _next = next;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogTrace("BaseUrlMiddleware processing");
            if (context != null)
            {
                var request = context.Request;

                if (_options.PublicOrigin.IsPresent())
                {
                    context.SetIdentityServerOrigin(_options.PublicOrigin);
                }

                context.SetIdentityServerBasePath(request.PathBase.Value.RemoveTrailingSlash());
            }

            _logger.LogTrace("BaseUrlMiddleware completed");
            await _next(context);
        }
    }
}