// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace sample
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // signing keys are protected with data protection; all instances of a farm must share these keys
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Environment.ContentRootPath, "dataprotectionkeys")));
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

                    options.KeyManagement.KeyPath = Path.Combine(Environment.ContentRootPath, "signingkeys");
                })
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients());
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
