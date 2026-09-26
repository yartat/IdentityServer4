// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// Signing credentials and validation keys from automatic key management.
    /// </summary>
    public interface IAutomaticKeyManagerKeyStore
    {
        /// <summary>
        /// Returns the credentials of the first configured algorithm, or null if key management is disabled.
        /// </summary>
        Task<SigningCredentials> GetSigningCredentialsAsync();

        /// <summary>
        /// Returns the credentials of all configured algorithms (empty if key management is disabled).
        /// </summary>
        Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync();

        /// <summary>
        /// Returns all keys to publish in the discovery document (empty if key management is disabled).
        /// </summary>
        Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync();
    }
}
