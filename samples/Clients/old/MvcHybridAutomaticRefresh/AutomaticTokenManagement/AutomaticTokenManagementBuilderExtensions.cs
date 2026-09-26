// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace MvcHybrid.AutomaticTokenManagement
{
    public static class AutomaticTokenManagementBuilderExtensions
    {
        public static AuthenticationBuilder AddAutomaticTokenManagement(this AuthenticationBuilder builder, Action<AutomaticTokenManagementOptions> options)
        {
            builder.Services.Configure(options);
            return builder.AddAutomaticTokenManagement();
        }

        public static AuthenticationBuilder AddAutomaticTokenManagement(this AuthenticationBuilder builder)
        {
            builder.Services.AddHttpClient("tokenClient");
            builder.Services.TryAddSingleton(TimeProvider.System);
            builder.Services.AddTransient<TokenEndpointService>();

            builder.Services.AddTransient<AutomaticTokenManagementCookieEvents>();
            builder.Services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, AutomaticTokenManagementConfigureCookieOptions>();

            return builder;
        }
    }
}