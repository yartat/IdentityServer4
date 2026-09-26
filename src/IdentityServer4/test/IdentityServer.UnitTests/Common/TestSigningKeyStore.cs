// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace IdentityServer.UnitTests.Common
{
    internal class TestSigningKeyStore : ISigningKeyStore
    {
        private int _loadCount;

        public ConcurrentDictionary<string, SerializedKey> Keys { get; } = new ConcurrentDictionary<string, SerializedKey>();

        public List<string> DeletedKeys { get; } = new List<string>();

        public int LoadCount => _loadCount;

        public Task<IEnumerable<SerializedKey>> LoadKeysAsync()
        {
            Interlocked.Increment(ref _loadCount);
            return Task.FromResult<IEnumerable<SerializedKey>>(Keys.Values.ToList());
        }

        public Task StoreKeyAsync(SerializedKey key)
        {
            Keys[key.Id] = key;
            return Task.CompletedTask;
        }

        public Task DeleteKeyAsync(string id)
        {
            lock (DeletedKeys) DeletedKeys.Add(id);
            Keys.TryRemove(id, out _);
            return Task.CompletedTask;
        }
    }
}
