// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Hosting
{
    /// <summary>
    ///     Middleware for re-writing the MTLS enabled endpoints to the standard protocol endpoints
    /// </summary>
    public class MutualTlsEndpointMiddleware
    {
        private readonly ILogger<MutualTlsEndpointMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutualTlsEndpointMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        /// <exception cref="ArgumentNullException">logger</exception>
        public MutualTlsEndpointMiddleware(
            RequestDelegate next,
            IdentityServerOptions options,
            ILogger<MutualTlsEndpointMiddleware> logger)
        {
            _next = next;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task Invoke(HttpContext context)
        {
            _logger.LogTrace("Mutual TLS middleware processing");
            if (_options.MutualTls.Enabled)
            {
                // domain-based MTLS
                _logger.LogTrace("Mutual TLS is enabled");
                if (_options.MutualTls.DomainName.IsPresent())
                {
                    // separate domain
                    _logger.LogTrace("Mutual TLS check domains");
                    if (_options.MutualTls.DomainName.Contains("."))
                    {
                        if (context.Request.Host.Host.Equals(_options.MutualTls.DomainName,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogTrace("Mutual TLS detects {domain} to validate certificate", _options.MutualTls.DomainName);
                            var result = await TriggerCertificateAuthentication(context);
                            if (!result.Succeeded)
                            {
                                _logger.LogTrace("Mutual TLS does not authenticate certificate {domain}", _options.MutualTls.DomainName);
                                return;
                            }
                        }
                    }
                    // sub-domain
                    else
                    {
                        if (context.Request.Host.Host.StartsWith(_options.MutualTls.DomainName + ".", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogTrace("Mutual TLS detects subdomain {domain} to validate certificate", _options.MutualTls.DomainName);
                            var result = await TriggerCertificateAuthentication(context);
                            if (!result.Succeeded)
                            {
                                _logger.LogTrace("Mutual TLS does not authenticate certificate {domain}", _options.MutualTls.DomainName);
                                return;
                            }
                        }
                    }
                }
                // path based MTLS
                else if (context.Request.Path.StartsWithSegments(Constants.ProtocolRoutePaths.MtlsPathPrefix.EnsureLeadingSlash(), out var subPath))
                {
                    _logger.LogTrace("Mutual TLS detects path based MTLS {path}", subPath);
                    var result = await TriggerCertificateAuthentication(context);
                    if (result.Succeeded)
                    {
                        var path = Constants.ProtocolRoutePaths.ConnectPathPrefix +
                                   subPath.ToString().EnsureLeadingSlash();
                        path = path.EnsureLeadingSlash();
                        _logger.LogDebug("Rewriting MTLS request from: {oldPath} to: {newPath}",
                            context.Request.Path.ToString(), path);
                        context.Request.Path = path;
                    }
                    else
                    {
                        _logger.LogTrace("Mutual TLS does not authenticate certificate {path}", subPath);
                        return;
                    }
                }
            }

            _logger.LogTrace("Mutual TLS middleware call next stage pipeline");
            await _next(context);
        }

        private async Task<AuthenticateResult> TriggerCertificateAuthentication(HttpContext context)
        {
            var x509AuthResult =
                await context.AuthenticateAsync(_options.MutualTls.ClientCertificateAuthenticationScheme);

            if (!x509AuthResult.Succeeded)
            {
                _logger.LogDebug("MTLS authentication failed, error: {error}.",
                    x509AuthResult.Failure?.Message);
                await context.ForbidAsync(_options.MutualTls.ClientCertificateAuthenticationScheme);
            }

            return x509AuthResult;
        }
    }
}