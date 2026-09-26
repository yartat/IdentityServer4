// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Signing key created by automatic key management, in the form it is persisted.
    /// </summary>
    public class SerializedKey
    {
        /// <summary>
        /// Version of the serialization format.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Key identifier (the "kid" in the JWKS and in token headers).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Time (UTC) the key was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Signing algorithm the key is used with.
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// Serialized key material, including the private key.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Whether <see cref="Data"/> is protected with ASP.NET Core data protection.
        /// </summary>
        public bool DataProtected { get; set; }
    }
}
