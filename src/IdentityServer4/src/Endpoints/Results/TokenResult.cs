// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    internal class TokenResult : IEndpointResult
    {
        private ILogger<TokenResult>? _logger;

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

        [MemberNotNull(nameof(_logger))]
        private void Initialize(HttpContext context)
        {
            _logger ??= context.RequestServices.GetRequiredService<ILogger<TokenResult>>();
        }

        internal class ResultDto
        {
#pragma warning disable CS8618, IDE1006 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable, Naming Styles
            public required string id_token { get; set; }

            public required string access_token { get; set; }

            public required int expires_in { get; set; }

            public required string token_type { get; set; }

            public required string refresh_token { get; set; }

            public required string scope { get; set; }

            [JsonExtensionData]
            public Dictionary<string, object>? Custom { get; set; }
        }
 #pragma warning restore CS8618, IDE1006 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
   }
}