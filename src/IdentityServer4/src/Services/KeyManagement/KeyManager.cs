// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// Default key manager. For every configured algorithm a key goes through these stages, measured by its age:
    /// announced (younger than PropagationTime) → signing (until RotationInterval) → validation only
    /// (until RotationInterval + RetentionDuration) → retired. A successor is created PropagationTime before
    /// the current key stops signing, so it is announced long enough when it takes over.
    /// </summary>
    public class KeyManager : IKeyManager
    {
        // serializes key creation within the process; instances of a farm converge through the store
        private static readonly SemaphoreSlim CreationLock = new SemaphoreSlim(1, 1);

        private readonly KeyManagementOptions _options;
        private readonly ISigningKeyStore _store;
        private readonly ISigningKeyStoreCache _cache;
        private readonly ISigningKeyProtector _protector;
        private readonly TimeProvider _clock;
        private readonly ILogger<KeyManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyManager"/> class.
        /// </summary>
        public KeyManager(
            IdentityServerOptions options,
            ISigningKeyStore store,
            ISigningKeyStoreCache cache,
            ISigningKeyProtector protector,
            TimeProvider clock,
            ILogger<KeyManager> logger)
        {
            _options = options.KeyManagement;
            _options.Validate();

            _store = store;
            _cache = cache;
            _protector = protector;
            _clock = clock;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<KeyContainer>> GetCurrentKeysAsync()
        {
            var keys = await GetKeysAsync();
            var now = _clock.GetUtcNow().UtcDateTime;

            return _options.SigningAlgorithms.Select(algorithm => SelectCurrentKey(keys, algorithm, now)).Where(key => key != null).ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<KeyContainer>> GetAllKeysAsync()
        {
            return await GetKeysAsync();
        }

        private async Task<IReadOnlyList<KeyContainer>> GetKeysAsync()
        {
            var now = _clock.GetUtcNow().UtcDateTime;

            var keys = await _cache.GetKeysAsync() ?? await LoadKeysAsync(now);
            keys = keys.Where(key => !IsRetired(key.Created, now)).ToList();

            if (!GetAlgorithmsNeedingNewKey(keys, now).Any())
            {
                return keys;
            }

            await CreationLock.WaitAsync();
            try
            {
                // another thread or instance may have created the keys in the meantime
                now = _clock.GetUtcNow().UtcDateTime;
                keys = await LoadKeysAsync(now);

                var algorithms = GetAlgorithmsNeedingNewKey(keys, now).ToList();
                if (algorithms.Count == 0)
                {
                    return keys;
                }

                var initializing = algorithms.Any(algorithm => SelectCurrentKey(keys, algorithm, now) == null);
                foreach (var algorithm in algorithms)
                {
                    await CreateKeyAsync(algorithm, now);
                }

                if (initializing && _options.InitializationSynchronizationDelay > TimeSpan.Zero)
                {
                    // instances starting at the same time may all have created a key; after the delay all of them
                    // see every key and select the same one (the oldest)
                    _logger.LogDebug("Waiting {delay} for other instances to create signing keys", _options.InitializationSynchronizationDelay);
                    await Task.Delay(_options.InitializationSynchronizationDelay, _clock);
                }

                return await LoadKeysAsync(_clock.GetUtcNow().UtcDateTime);
            }
            finally
            {
                CreationLock.Release();
            }
        }

        private async Task<IReadOnlyList<KeyContainer>> LoadKeysAsync(DateTime now)
        {
            var keys = new List<KeyContainer>();

            foreach (var serializedKey in await _store.LoadKeysAsync())
            {
                if (IsRetired(serializedKey.Created, now))
                {
                    if (_options.DeleteRetiredKeys)
                    {
                        await DeleteKeyAsync(serializedKey.Id);
                    }

                    continue;
                }

                try
                {
                    keys.Add(_protector.Unprotect(serializedKey));
                }
                catch (Exception ex)
                {
                    // e.g. the data protection keys changed or are not shared by all instances
                    _logger.LogError(ex, "Signing key {kid} could not be unprotected and is ignored", serializedKey.Id);
                }
            }

            var cacheDuration = keys.Any(key => now - key.Created < _options.InitializationDuration)
                ? _options.InitializationKeyCacheDuration
                : _options.KeyCacheDuration;
            await _cache.StoreKeysAsync(keys, cacheDuration);

            _logger.LogTrace("Loaded {count} signing keys, cached for {duration}", keys.Count, cacheDuration);
            return keys;
        }

        private async Task CreateKeyAsync(string algorithm, DateTime now)
        {
            var id = CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex);
            var key = new KeyContainer(id, algorithm, now, KeyMaterial.Create(algorithm, id, _options.RsaKeySize));

            await _store.StoreKeyAsync(_protector.Protect(key));

            _logger.LogInformation("Created signing key {kid} for {algorithm}", id, algorithm);
        }

        private async Task DeleteKeyAsync(string id)
        {
            try
            {
                await _store.DeleteKeyAsync(id);
                _logger.LogInformation("Deleted retired signing key {kid}", id);
            }
            catch (Exception ex)
            {
                // e.g. deleted by another instance; retired keys are ignored anyway
                _logger.LogWarning(ex, "Failed to delete retired signing key {kid}", id);
            }
        }

        private IEnumerable<string> GetAlgorithmsNeedingNewKey(IReadOnlyList<KeyContainer> keys, DateTime now)
        {
            foreach (var algorithm in _options.SigningAlgorithms)
            {
                var current = SelectCurrentKey(keys, algorithm, now);
                if (current == null)
                {
                    yield return algorithm;
                    continue;
                }

                // a successor has to outlive the current key; keys created at about the same time (e.g. by instances
                // that started together) are due for rotation themselves and do not count
                var rotationAge = _options.RotationInterval - _options.PropagationTime;
                var hasSuccessor = keys.Any(key => key.Algorithm == algorithm && IsNewer(key, current) && now - key.Created < rotationAge);
                if (!hasSuccessor && now - current.Created >= rotationAge)
                {
                    yield return algorithm;
                }
            }
        }

        private KeyContainer SelectCurrentKey(IReadOnlyList<KeyContainer> keys, string algorithm, DateTime now)
        {
            var candidates = keys
                .Where(key => key.Algorithm == algorithm && now - key.Created < _options.RotationInterval)
                .OrderBy(key => key.Created)
                .ThenBy(key => key.Id, StringComparer.Ordinal)
                .ToList();

            // the newest key announced long enough; if none is (first start, or the server was down past a rotation),
            // the key announced longest has to be used right away
            return candidates.LastOrDefault(key => now - key.Created >= _options.PropagationTime) ?? candidates.FirstOrDefault();
        }

        private bool IsRetired(DateTime created, DateTime now) => now - created >= _options.RotationInterval + _options.RetentionDuration;

        private static bool IsNewer(KeyContainer key, KeyContainer other) =>
            key.Created > other.Created || (key.Created == other.Created && string.CompareOrdinal(key.Id, other.Id) > 0);
    }
}
