// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer4.Hosting;

/// <summary>
/// Middleware for configuring the base URL and public origin of IdentityServer.
/// </summary>
/// <remarks>
/// This middleware is responsible for setting up the IdentityServer's base URL configuration
/// for each request. It applies the configured public origin and base path to the HTTP context,
/// which are used throughout the request pipeline for generating URLs and determining the 
/// IdentityServer's authority URI.
/// </remarks>
/// <seealso cref="IMiddleware" />
public class BaseUrlMiddleware(
    RequestDelegate next,
    IdentityServerOptions options,
    ILogger<BaseUrlMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly IdentityServerOptions _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<BaseUrlMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Invoke(HttpContext context)
    {
        _logger.LogTrace("BaseUrlMiddleware processing");
        if (context is not null)
        {
            var request = context.Request;

            if (_options.PublicOrigin.IsPresent())
            {
                context.SetIdentityServerOrigin(_options.PublicOrigin);
            }

            context.SetIdentityServerBasePath(request.PathBase.Value.RemoveTrailingSlash());
        }

        _logger.LogTrace("BaseUrlMiddleware completed");
        await _next(context!);
    }
}