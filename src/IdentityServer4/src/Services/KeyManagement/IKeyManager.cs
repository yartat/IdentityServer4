// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// Creates, rotates and retires signing keys.
    /// </summary>
    public interface IKeyManager
    {
        /// <summary>
        /// Returns the keys to sign with, one per configured signing algorithm, in configuration order.
        /// Creates keys as needed.
        /// </summary>
        Task<IEnumerable<KeyContainer>> GetCurrentKeysAsync();

        /// <summary>
        /// Returns all keys that are not retired: keys being announced, the current keys and keys kept for validation.
        /// Creates keys as needed.
        /// </summary>
        Task<IEnumerable<KeyContainer>> GetAllKeysAsync();
    }
}
