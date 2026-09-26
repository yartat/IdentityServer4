// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// Signing key created by automatic key management.
    /// </summary>
    public class KeyContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyContainer"/> class.
        /// </summary>
        /// <param name="id">The key identifier.</param>
        /// <param name="algorithm">The signing algorithm.</param>
        /// <param name="created">The creation time (UTC).</param>
        /// <param name="key">The key, including the private key.</param>
        public KeyContainer(string id, string algorithm, DateTime created, AsymmetricSecurityKey key)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            Created = created;
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Key identifier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Signing algorithm.
        /// </summary>
        public string Algorithm { get; }

        /// <summary>
        /// Creation time (UTC).
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        /// The key, including the private key.
        /// </summary>
        public AsymmetricSecurityKey Key { get; }

        /// <summary>
        /// Credentials for signing with this key.
        /// </summary>
        public SigningCredentials ToSigningCredentials() => new SigningCredentials(Key, Algorithm);

        /// <summary>
        /// Key information for publishing this key in the discovery document.
        /// </summary>
        public SecurityKeyInfo ToSecurityKeyInfo() => new SecurityKeyInfo { Key = Key, SigningAlgorithm = Algorithm };
    }
}
