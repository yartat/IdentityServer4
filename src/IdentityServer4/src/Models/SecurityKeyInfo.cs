// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Information about a security key
    /// </summary>
    public class SecurityKeyInfo
    {
        /// <summary>
        /// The key
        /// </summary>
        public SecurityKey Key { get; set; }

        /// <summary>
        /// The signing algorithm
        /// </summary>
        public string SigningAlgorithm { get; set; }
    }
}