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
using Microsoft.AspNetCore.HttpOverrides;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SampleApi
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

            // this API will accept any access token from the authority
            services.AddAuthentication("token")
                .AddJwtBearer("token", options =>
                {
                    options.Authority = Constants.Authority;
                    options.TokenValidationParameters.ValidateAudience = false;
                    
                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                });
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
    }
}