// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.EntityFramework.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using sample;

namespace migrations
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cn = Configuration.GetConnectionString("db");
            var migrationsAssembly = typeof(Startup).Assembly.FullName;

            services.AddOperationalDbContext(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(cn, sql => sql.MigrationsAssembly(migrationsAssembly));
            });
            services.AddDbContext<DataProtectionKeysDbContext>(b => b.UseSqlServer(cn, sql => sql.MigrationsAssembly(migrationsAssembly)));
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }
}
