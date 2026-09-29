// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace sample
{
    public class Startup
    {
        // the "migrations" project holds the migrations of both contexts; run its builddb.bat to create the database
        public const string MigrationsAssembly = "migrations";

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration config, IWebHostEnvironment environment)
        {
            Configuration = config;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cn = Configuration.GetConnectionString("db");

            // signing keys are protected with data protection; storing its keys in the database shares them with all instances
            services.AddDbContext<DataProtectionKeysDbContext>(b => b.UseSqlServer(cn, sql => sql.MigrationsAssembly(MigrationsAssembly)));
            services.AddDataProtection()
                .PersistKeysToDbContext<DataProtectionKeysDbContext>();
                //.ProtectKeysWithCertificate(cert);

            services.AddIdentityServer(options =>
                {
                    options.KeyManagement.Enabled = true;

                    // all of these values are shortened for local testing, so rotation can be watched in the JWKS;
                    // the defaults rotate every 90 days, announce new keys 14 days ahead and keep old keys 14 days
                    options.KeyManagement.RotationInterval = TimeSpan.FromMinutes(6);
                    options.KeyManagement.PropagationTime = TimeSpan.FromMinutes(2);
                    options.KeyManagement.RetentionDuration = TimeSpan.FromMinutes(2);
                    options.KeyManagement.KeyCacheDuration = TimeSpan.FromSeconds(30);
                    options.KeyManagement.InitializationDuration = TimeSpan.FromSeconds(30);
                    options.KeyManagement.InitializationKeyCacheDuration = TimeSpan.FromSeconds(10);
                    options.KeyManagement.InitializationSynchronizationDelay = TimeSpan.FromSeconds(1);
                })
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients())
                // the operational store also stores the signing keys (Keys table)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(cn, sql => sql.MigrationsAssembly(MigrationsAssembly));
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
        }
    }
}
