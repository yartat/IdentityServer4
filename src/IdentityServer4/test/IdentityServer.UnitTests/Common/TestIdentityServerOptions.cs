// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;

namespace IdentityServer.UnitTests.Common
{
    internal class TestIdentityServerOptions
    {
        public static IdentityServerOptions Create()
        {
            var options = new IdentityServerOptions
            {
                IssuerUri = "https://idsvr.com"
            };

            return options;
        }
    }
}