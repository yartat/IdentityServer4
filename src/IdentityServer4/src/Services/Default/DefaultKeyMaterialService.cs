// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using IdentityServer4.Stores;
using IdentityServer4.Models;
using System.Linq;
using System;
using IdentityServer4.Extensions;
using IdentityServer4.Services.KeyManagement;

namespace IdentityServer4.Services
{
    /// <summary>
    /// The default key material service
    /// </summary>
    /// <seealso cref="IdentityServer4.Services.IKeyMaterialService" />
    public class DefaultKeyMaterialService : IKeyMaterialService
    {
        private readonly IEnumerable<ISigningCredentialStore> _signingCredentialStores;
        private readonly IEnumerable<IValidationKeysStore> _validationKeysStores;
        private readonly IAutomaticKeyManagerKeyStore _keyManagerKeyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultKeyMaterialService"/> class.
        /// </summary>
        /// <param name="validationKeysStores">The validation keys stores.</param>
        /// <param name="signingCredentialStores">The signing credential store.</param>
        public DefaultKeyMaterialService(IEnumerable<IValidationKeysStore> validationKeysStores, IEnumerable<ISigningCredentialStore> signingCredentialStores)
            : this(validationKeysStores, signingCredentialStores, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultKeyMaterialService"/> class.
        /// </summary>
        /// <param name="validationKeysStores">The validation keys stores.</param>
        /// <param name="signingCredentialStores">The signing credential store.</param>
        /// <param name="keyManagerKeyStore">The keys of automatic key management.</param>
        public DefaultKeyMaterialService(
            IEnumerable<IValidationKeysStore> validationKeysStores,
            IEnumerable<ISigningCredentialStore> signingCredentialStores,
            IAutomaticKeyManagerKeyStore keyManagerKeyStore)
        {
            _signingCredentialStores = signingCredentialStores;
            _validationKeysStores = validationKeysStores;
            _keyManagerKeyStore = keyManagerKeyStore;
        }

        /// <inheritdoc/>
        public async Task<SigningCredentials> GetSigningCredentialsAsync(IEnumerable<string> allowedAlgorithms = null)
        {
            if (allowedAlgorithms.IsNullOrEmpty())
            {
                // static credentials take precedence over automatically managed keys
                if (_signingCredentialStores.Any())
                {
                    return await _signingCredentialStores.First().GetSigningCredentialsAsync();
                }

                return _keyManagerKeyStore == null ? null : await _keyManagerKeyStore.GetSigningCredentialsAsync();
            }

            var credentials = (await GetAllSigningCredentialsAsync()).ToList();
            if (credentials.Count == 0)
            {
                return null;
            }

            var credential = credentials.FirstOrDefault(c => allowedAlgorithms.Contains(c.Algorithm));
            if (credential is null)
            {
                throw new InvalidOperationException($"No signing credential for algorithms ({allowedAlgorithms.ToSpaceSeparatedString()}) registered.");
            }

            return credential;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync()
        {
            var credentials = new List<SigningCredentials>();

            foreach (var store in _signingCredentialStores)
            {
                credentials.Add(await store.GetSigningCredentialsAsync());
            }

            if (_keyManagerKeyStore != null)
            {
                credentials.AddRange(await _keyManagerKeyStore.GetAllSigningCredentialsAsync());
            }

            return credentials;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var keys = new List<SecurityKeyInfo>();

            foreach (var store in _validationKeysStores)
            {
                keys.AddRange(await store.GetValidationKeysAsync());
            }

            if (_keyManagerKeyStore != null)
            {
                keys.AddRange(await _keyManagerKeyStore.GetValidationKeysAsync());
            }

            return keys;
        }
    }
}
