// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace ResourceBasedApi
{
    /// <summary>
    /// OpenAPI document (Microsoft.AspNetCore.OpenApi) and Scalar UI for this API.
    /// The document describes IdentityServer's client credentials flow, so Scalar can request an access token.
    /// </summary>
    internal static class OpenApiExtensions
    {
        private const string SecuritySchemeName = "oauth2";

        public static IServiceCollection AddOpenApiWithClientCredentials(this IServiceCollection services, string authority, IReadOnlyDictionary<string, string> scopes) =>
            services.AddOpenApi(options => options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[SecuritySchemeName] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Description = "Access token issued by IdentityServer",
                    Flows = new OpenApiOAuthFlows
                    {
                        ClientCredentials = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri(authority.TrimEnd('/') + "/connect/token"),
                            Scopes = scopes.ToDictionary(x => x.Key, x => x.Value)
                        }
                    }
                };

                document.Security ??= new List<OpenApiSecurityRequirement>();
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(SecuritySchemeName, document)] = scopes.Keys.ToList()
                });

                return Task.CompletedTask;
            }));

        /// <summary>
        /// Maps the OpenAPI document at /openapi/v1.json and the Scalar UI at /scalar/v1.
        /// </summary>
        public static void MapOpenApiWithScalar(this IEndpointRouteBuilder endpoints, string clientId, IEnumerable<string> scopes)
        {
            endpoints.MapOpenApi();
            endpoints.MapScalarApiReference(options => options
                .AddPreferredSecuritySchemes([SecuritySchemeName])
                .AddClientCredentialsFlow(SecuritySchemeName, flow => flow
                    .WithClientId(clientId)
                    .WithSelectedScopes(scopes)));
        }
    }
}
