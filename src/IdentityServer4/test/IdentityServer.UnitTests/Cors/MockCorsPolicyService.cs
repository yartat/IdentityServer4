// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Services;

namespace IdentityServer.UnitTests.Cors
{
    public class MockCorsPolicyService : ICorsPolicyService
    {
        public bool WasCalled { get; set; }
        public bool Response { get; set; }

        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            WasCalled = true;
            return Task.FromResult(Response);
        }
    }
}
