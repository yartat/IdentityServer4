// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace IdentityServer4.EntityFramework.Entities
{
#pragma warning disable 1591
    public class Key
    {
        public string Id { get; set; }

        public int Version { get; set; }

        public DateTime Created { get; set; }

        public string Algorithm { get; set; }

        public bool DataProtected { get; set; }

        public string Data { get; set; }
    }
}
