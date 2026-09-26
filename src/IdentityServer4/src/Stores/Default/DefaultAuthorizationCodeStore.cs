// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Default authorization code store.
    /// </summary>
    public class DefaultAuthorizationCodeStore : DefaultGrantStore<AuthorizationCode>, IAuthorizationCodeStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAuthorizationCodeStore"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="handleGenerationService">The handle generation service.</param>
        /// <param name="logger">The logger.</param>
        public DefaultAuthorizationCodeStore(
            IPersistedGrantStore store,
            IPersistentGrantSerializer serializer,
            IHandleGenerationService handleGenerationService,
            ILogger<DefaultAuthorizationCodeStore> logger)
            : base(IdentityServerConstants.PersistedGrantTypes.AuthorizationCode, store, serializer, handleGenerationService, logger)
        {
        }

        /// <inheritdoc/>
        public Task<string> StoreAuthorizationCodeAsync(AuthorizationCode code) =>
            CreateItemAsync(code, code.ClientId, code.Subject.GetSubjectId(), code.SessionId, code.Description, code.CreationTime, code.Lifetime);

        /// <inheritdoc/>
        public Task StoreAuthorizationCodeAsync(string handle, AuthorizationCode code) =>
            StoreItemAsync(handle, code, code.ClientId, code.Subject.GetSubjectId(), code.SessionId, code.Description, code.CreationTime, code.CreationTime.AddSeconds(code.Lifetime));

        /// <inheritdoc/>
        public Task<AuthorizationCode> GetAuthorizationCodeAsync(string code) =>
            GetItemAsync(code);

        /// <inheritdoc/>
        public async Task<AuthorizationCode> GetAndRemoveAuthorizationCodeAsync(string code)
        {
            var grant = await Store.GetAndRemoveAsync(GetHashedKey(code));
            if (grant != null && grant.Type == GrantType)
            {
                try
                {
                    return Serializer.Deserialize<AuthorizationCode>(grant.Data);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to deserialize JSON from grant store.");
                }
            }
            else
            {
                Logger.LogDebug("{grantType} grant with value: {key} not found in store.", GrantType, code);
            }

            return default;
        }

        /// <inheritdoc/>
        public Task RemoveAuthorizationCodeAsync(string code) =>
            RemoveItemAsync(code);
    }
}