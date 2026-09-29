// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for the store of signing keys created by automatic key management
    /// </summary>
    public interface ISigningKeyStore
    {
        /// <summary>
        /// Returns all keys in the store.
        /// </summary>
        Task<IEnumerable<SerializedKey>> LoadKeysAsync();

        /// <summary>
        /// Stores a new key.
        /// </summary>
        /// <param name="key">The key.</param>
        Task StoreKeyAsync(SerializedKey key);

        /// <summary>
        /// Deletes the key with the given identifier.
        /// </summary>
        /// <param name="id">The key identifier.</param>
        Task DeleteKeyAsync(string id);
    }
}
