// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// Cache of the keys loaded from the <see cref="Stores.ISigningKeyStore"/>.
    /// </summary>
    public interface ISigningKeyStoreCache
    {
        /// <summary>
        /// Returns the cached keys, or null if nothing is cached or the cache expired.
        /// </summary>
        Task<IReadOnlyList<KeyContainer>> GetKeysAsync();

        /// <summary>
        /// Caches the keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="duration">How long the keys are cached.</param>
        Task StoreKeysAsync(IReadOnlyList<KeyContainer> keys, TimeSpan duration);
    }
}
