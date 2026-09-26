// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Duende.IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    internal class TokenResult : IEndpointResult
    {
        private ILogger<TokenResult> _logger;

        public TokenResult(TokenResponse response)
        {
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public TokenResponse Response { get; set; }

        public async Task ExecuteAsync(HttpContext context)
        {
            Initialize(context);

            _logger.LogTrace("Token result processing");
            context.Response.SetNoCache();

            var dto = new ResultDto
            {
                id_token = Response.IdentityToken,
                access_token = Response.AccessToken,
                refresh_token = Response.RefreshToken,
                expires_in = Response.AccessTokenLifetime,
                token_type = OidcConstants.TokenResponse.BearerTokenType,
                scope = Response.Scope,

                Custom = Response.Custom
            };

            await context.Response.WriteJsonAsync(dto);
            _logger.LogTrace("Token result completed");
        }

        private void Initialize(HttpContext context)
        {
            _logger ??= context.RequestServices.GetRequiredService<ILogger<TokenResult>>();
        }

        internal class ResultDto
        {
            public string id_token { get; set; }

            public string access_token { get; set; }

            public int expires_in { get; set; }

            public string token_type { get; set; }

            public string refresh_token { get; set; }

            public string scope { get; set; }

            [JsonExtensionData]
            public Dictionary<string, object> Custom { get; set; }
        }
    }
}