// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Duende.AspNetCore.Authentication.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ResourceBasedApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // OpenAPI document describing how to obtain a token from IdentityServer; rendered by Scalar in development
            services.AddOpenApiWithClientCredentials(Constants.Authority, new Dictionary<string, string> { ["resource1.scope1"] = "Resource 1, scope 1" });

            services.AddCors();
            services.AddDistributedMemoryCache();

            services.AddAuthentication("token")

                // JWT tokens
                .AddJwtBearer("token", options =>
                {
                    options.Authority = Constants.Authority;
                    options.Audience = "resource1";

                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };

                    // if token does not contain a dot, it is a reference token
                    options.ForwardDefaultSelector = context => ForwardReferenceToken(context, "introspection");
                })

                // reference tokens
                .AddOAuth2Introspection("introspection", options =>
                {
                    options.Authority = Constants.Authority;

                    options.ClientId = "resource1";
                    options.ClientSecret = "secret";
                });

            services.AddTransient<IClaimsTransformation, ScopeClaimsTransformation>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(policy =>
            {
                policy.WithOrigins(
                    "https://localhost:44300");

                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithExposedHeaders("WWW-Authenticate");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization();

                if (env.IsDevelopment())
                {
                    endpoints.MapOpenApiWithScalar("client", ["resource1.scope1"]);
                }
            });
        }

        // a JWT has dot-separated segments; any other bearer token is a reference token and goes to introspection
        private static string ForwardReferenceToken(HttpContext context, string introspectionScheme)
        {
            var authorization = context.Request.Headers.Authorization.ToString();
            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var token = authorization.Substring("Bearer ".Length).Trim();
            return token.Contains('.') ? null : introspectionScheme;
        }
    }
}
