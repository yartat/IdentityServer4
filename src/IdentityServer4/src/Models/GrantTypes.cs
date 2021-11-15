// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace IdentityServer4.Models
{
#pragma warning disable 1591
    public static class GrantTypes
    {
        public static ISet<string> Implicit =>
            new HashSet<string> { GrantType.Implicit };

        public static ISet<string> ImplicitAndClientCredentials =>
            new HashSet<string> { GrantType.Implicit, GrantType.ClientCredentials };

        public static ISet<string> Code =>
            new HashSet<string> { GrantType.AuthorizationCode };

        public static ISet<string> CodeAndClientCredentials =>
            new HashSet<string> { GrantType.AuthorizationCode, GrantType.ClientCredentials };

        public static ISet<string> Hybrid =>
            new HashSet<string> { GrantType.Hybrid };

        public static ISet<string> HybridAndClientCredentials =>
            new HashSet<string> { GrantType.Hybrid, GrantType.ClientCredentials };

        public static ISet<string> ClientCredentials =>
            new HashSet<string> { GrantType.ClientCredentials };

        public static ISet<string> ResourceOwnerPassword =>
            new HashSet<string> { GrantType.ResourceOwnerPassword };

        public static ISet<string> ResourceOwnerPasswordAndClientCredentials =>
            new HashSet<string> { GrantType.ResourceOwnerPassword, GrantType.ClientCredentials };

        public static ISet<string> DeviceFlow =>
            new HashSet<string> { GrantType.DeviceFlow };
    }
#pragma warning restore 1591
}