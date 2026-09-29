// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// Default <see cref="IAutomaticKeyManagerKeyStore"/> on top of the <see cref="IKeyManager"/>.
    /// </summary>
    public class AutomaticKeyManagerKeyStore : IAutomaticKeyManagerKeyStore
    {
        private readonly KeyManagementOptions _options;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticKeyManagerKeyStore"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="serviceProvider">Resolves the <see cref="IKeyManager"/> only when key management is enabled.</param>
        public AutomaticKeyManagerKeyStore(IdentityServerOptions options, IServiceProvider serviceProvider)
        {
            _options = options.KeyManagement;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            return (await GetAllSigningCredentialsAsync()).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync()
        {
            if (!_options.Enabled)
            {
                return Enumerable.Empty<SigningCredentials>();
            }

            var keys = await KeyManager.GetCurrentKeysAsync();
            return keys.Select(key => key.ToSigningCredentials()).ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            if (!_options.Enabled)
            {
                return Enumerable.Empty<SecurityKeyInfo>();
            }

            var keys = await KeyManager.GetAllKeysAsync();
            return keys.Select(key => key.ToSecurityKeyInfo()).ToList();
        }

        private IKeyManager KeyManager => _serviceProvider.GetRequiredService<IKeyManager>();
    }
}
