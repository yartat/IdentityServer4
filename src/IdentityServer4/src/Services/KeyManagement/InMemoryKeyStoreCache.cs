// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// In-process cache of signing keys. Keys stay unprotected in memory only.
    /// </summary>
    public class InMemoryKeyStoreCache : ISigningKeyStoreCache
    {
        private readonly TimeProvider _clock;
        private readonly object _lock = new object();

        private IReadOnlyList<KeyContainer> _keys;
        private DateTimeOffset _expires;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryKeyStoreCache"/> class.
        /// </summary>
        public InMemoryKeyStoreCache(TimeProvider clock)
        {
            _clock = clock;
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<KeyContainer>> GetKeysAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_keys != null && _clock.GetUtcNow() < _expires ? _keys : null);
            }
        }

        /// <inheritdoc/>
        public Task StoreKeysAsync(IReadOnlyList<KeyContainer> keys, TimeSpan duration)
        {
            lock (_lock)
            {
                _keys = keys;
                _expires = _clock.GetUtcNow() + duration;
            }

            return Task.CompletedTask;
        }
    }
}
