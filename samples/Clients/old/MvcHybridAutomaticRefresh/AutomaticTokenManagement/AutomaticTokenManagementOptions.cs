// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace MvcHybrid.AutomaticTokenManagement
{
    public class AutomaticTokenManagementOptions
    {
        public string Scheme { get; set; }
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);
        public bool RevokeRefreshTokenOnSignout { get; set; } = true;
    }
}