// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services.KeyManagement;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace IdentityServer.UnitTests.Services.KeyManagement
{
    public class KeyManagerTests
    {
        private const string Category = "KeyManager";

        private static readonly TimeSpan Rotation = TimeSpan.FromDays(90);
        private static readonly TimeSpan Propagation = TimeSpan.FromDays(14);
        private static readonly TimeSpan Retention = TimeSpan.FromDays(14);

        private readonly IdentityServerOptions _options = new IdentityServerOptions();
        private readonly TestSigningKeyStore _store = new TestSigningKeyStore();
        private readonly StubTimeProvider _clock = new StubTimeProvider();
        private readonly IDataProtectionProvider _dataProtectionProvider = new EphemeralDataProtectionProvider();

        public KeyManagerTests()
        {
            _options.KeyManagement.Enabled = true;
            _options.KeyManagement.RotationInterval = Rotation;
            _options.KeyManagement.PropagationTime = Propagation;
            _options.KeyManagement.RetentionDuration = Retention;
            _options.KeyManagement.InitializationSynchronizationDelay = TimeSpan.Zero;
        }

        private KeyManager CreateSubject(ISigningKeyStoreCache cache = null) => new KeyManager(
            _options,
            _store,
            cache ?? new InMemoryKeyStoreCache(_clock),
            new DataProtectionKeyProtector(_dataProtectionProvider, _options),
            _clock,
            TestLogger.Create<KeyManager>());

        // a fresh cache for every call, so the test sees exactly what is in the store
        private Task<KeyContainer> GetCurrentKeyAsync() => CreateSubject().GetCurrentKeysAsync().ContinueWith(t => t.Result.Single());

        private async Task<string[]> GetAllKeyIdsAsync() => (await CreateSubject().GetAllKeysAsync()).Select(x => x.Id).ToArray();

        private async Task<string> AddKeyAsync(TimeSpan age, string algorithm = SecurityAlgorithms.RsaSha256)
        {
            var created = _clock.GetUtcNow().UtcDateTime - age;
            var id = Guid.NewGuid().ToString("N");
            var key = algorithm.StartsWith("ES")
                ? (AsymmetricSecurityKey)new ECDsaSecurityKey(System.Security.Cryptography.ECDsa.Create(System.Security.Cryptography.ECCurve.NamedCurves.nistP256)) { KeyId = id }
                : new RsaSecurityKey(System.Security.Cryptography.RSA.Create(2048)) { KeyId = id };

            var protector = new DataProtectionKeyProtector(_dataProtectionProvider, _options);
            await _store.StoreKeyAsync(protector.Protect(new KeyContainer(id, algorithm, created, key)));
            return id;
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task when_no_keys_exist_a_key_should_be_created_and_used_immediately()
        {
            var key = await GetCurrentKeyAsync();

            _store.Keys.Should().ContainSingle();
            key.Id.Should().Be(_store.Keys.Keys.Single());
            key.Algorithm.Should().Be(SecurityAlgorithms.RsaSha256);
            key.Created.Should().Be(_clock.GetUtcNow().UtcDateTime);
            key.Key.Should().BeOfType<RsaSecurityKey>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task existing_key_should_be_reused()
        {
            var id = await AddKeyAsync(TimeSpan.FromDays(20));

            var key = await GetCurrentKeyAsync();

            key.Id.Should().Be(id);
            _store.Keys.Should().ContainSingle();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task no_successor_should_be_created_before_the_rotation_threshold()
        {
            await AddKeyAsync(Rotation - Propagation - TimeSpan.FromSeconds(1));

            await GetCurrentKeyAsync();

            _store.Keys.Should().ContainSingle();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task at_the_rotation_threshold_a_successor_should_be_announced_but_not_used()
        {
            var current = await AddKeyAsync(Rotation - Propagation);

            var key = await GetCurrentKeyAsync();

            key.Id.Should().Be(current);
            _store.Keys.Should().HaveCount(2);
            var successor = _store.Keys.Keys.Single(x => x != current);
            (await GetAllKeyIdsAsync()).Should().BeEquivalentTo(new[] { current, successor });
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task successor_should_take_over_when_the_current_key_expires()
        {
            var current = await AddKeyAsync(Rotation - Propagation);
            await GetCurrentKeyAsync();
            var successor = _store.Keys.Keys.Single(x => x != current);

            _clock.Advance(Propagation - TimeSpan.FromSeconds(1));
            (await GetCurrentKeyAsync()).Id.Should().Be(current);

            _clock.Advance(TimeSpan.FromSeconds(1));
            (await GetCurrentKeyAsync()).Id.Should().Be(successor);

            // the expired key is still published for validation, and no further key is created
            (await GetAllKeyIdsAsync()).Should().BeEquivalentTo(new[] { current, successor });
            _store.Keys.Should().HaveCount(2);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task full_lifecycle_should_always_have_a_propagated_signing_key()
        {
            var subject = CreateSubject();
            var first = (await subject.GetCurrentKeysAsync()).Single();

            string previous = first.Id;
            for (var day = 1; day <= 400; day++)
            {
                _clock.Advance(TimeSpan.FromDays(1));
                var current = await GetCurrentKeyAsync();
                var now = _clock.GetUtcNow().UtcDateTime;

                current.Created.Should().BeOnOrBefore(now);
                (now - current.Created).Should().BeLessThan(Rotation);
                if (current.Id != previous)
                {
                    // every key after the first one is announced for the full propagation time before it signs
                    (now - current.Created).Should().BeGreaterThanOrEqualTo(Propagation);
                    previous = current.Id;
                }

                _store.Keys.Count.Should().BeLessThanOrEqualTo(3);
            }
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task retired_keys_should_be_deleted_and_not_published()
        {
            var retired = await AddKeyAsync(Rotation + Retention);
            var validationOnly = await AddKeyAsync(Rotation + Retention - TimeSpan.FromSeconds(1));
            var current = await AddKeyAsync(Propagation);

            (await GetAllKeyIdsAsync()).Should().BeEquivalentTo(new[] { validationOnly, current });
            _store.DeletedKeys.Should().Equal(retired);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task retired_keys_should_be_kept_in_the_store_when_deletion_is_disabled()
        {
            _options.KeyManagement.DeleteRetiredKeys = false;
            var retired = await AddKeyAsync(Rotation + Retention);
            var current = await AddKeyAsync(Propagation);

            (await GetAllKeyIdsAsync()).Should().Equal(current);
            _store.DeletedKeys.Should().BeEmpty();
            _store.Keys.Keys.Should().Contain(retired);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task when_all_keys_expired_a_new_key_should_be_used_immediately()
        {
            var expired = await AddKeyAsync(Rotation + TimeSpan.FromDays(1));

            var key = await GetCurrentKeyAsync();

            key.Id.Should().NotBe(expired);
            key.Created.Should().Be(_clock.GetUtcNow().UtcDateTime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task keys_created_at_the_same_time_should_select_the_oldest_and_still_rotate()
        {
            // two instances started together and both created a key
            var older = await AddKeyAsync(TimeSpan.FromSeconds(2));
            var newer = await AddKeyAsync(TimeSpan.FromSeconds(1));

            (await GetCurrentKeyAsync()).Id.Should().Be(older);

            // both are announced long enough by now; either may sign, but a successor has to be created
            _clock.Advance(Rotation - Propagation);
            (await GetCurrentKeyAsync()).Id.Should().BeOneOf(older, newer);
            var successor = _store.Keys.Keys.Single(x => x != older && x != newer);

            _clock.Advance(Propagation);
            (await GetCurrentKeyAsync()).Id.Should().Be(successor);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task a_key_should_be_created_for_every_algorithm_in_configuration_order()
        {
            _options.KeyManagement.SigningAlgorithms = new[] { SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.RsaSsaPssSha256 };

            var keys = (await CreateSubject().GetCurrentKeysAsync()).ToList();

            keys.Select(x => x.Algorithm).Should().Equal(SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.RsaSsaPssSha256);
            keys[0].Key.Should().BeOfType<ECDsaSecurityKey>();
            keys[1].Key.Should().BeOfType<RsaSecurityKey>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task keys_of_removed_algorithms_should_be_published_but_not_used_for_signing()
        {
            var ec = await AddKeyAsync(Propagation, SecurityAlgorithms.EcdsaSha256);
            var rsa = await AddKeyAsync(Propagation);

            var subject = CreateSubject();

            (await subject.GetCurrentKeysAsync()).Select(x => x.Id).Should().Equal(rsa);
            (await subject.GetAllKeysAsync()).Select(x => x.Id).Should().BeEquivalentTo(new[] { ec, rsa });
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task keys_that_cannot_be_unprotected_should_be_ignored()
        {
            var foreign = new DataProtectionKeyProtector(new EphemeralDataProtectionProvider(), _options).Protect(
                new KeyContainer("foreign", SecurityAlgorithms.RsaSha256, _clock.GetUtcNow().UtcDateTime - Propagation,
                    new RsaSecurityKey(System.Security.Cryptography.RSA.Create(2048)) { KeyId = "foreign" }));
            await _store.StoreKeyAsync(foreign);

            var key = await GetCurrentKeyAsync();

            key.Id.Should().NotBe("foreign");
            _store.Keys.Keys.Should().Contain("foreign");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task concurrent_requests_should_create_a_single_key()
        {
            var cache = new InMemoryKeyStoreCache(_clock);

            var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() => CreateSubject(cache).GetCurrentKeysAsync())));

            _store.Keys.Should().ContainSingle();
            results.Select(x => x.Single().Id).Distinct().Should().ContainSingle();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task keys_should_be_served_from_the_cache_until_it_expires()
        {
            _options.KeyManagement.KeyCacheDuration = TimeSpan.FromHours(1);
            _options.KeyManagement.InitializationKeyCacheDuration = TimeSpan.FromMinutes(1);
            var cache = new InMemoryKeyStoreCache(_clock);
            await AddKeyAsync(Propagation);

            await CreateSubject(cache).GetCurrentKeysAsync();
            var loads = _store.LoadCount;
            var otherInstanceKey = await AddKeyAsync(TimeSpan.FromDays(1));

            _clock.Advance(TimeSpan.FromMinutes(59));
            (await CreateSubject(cache).GetAllKeysAsync()).Select(x => x.Id).Should().NotContain(otherInstanceKey);
            _store.LoadCount.Should().Be(loads);

            _clock.Advance(TimeSpan.FromMinutes(1));
            (await CreateSubject(cache).GetAllKeysAsync()).Select(x => x.Id).Should().Contain(otherInstanceKey);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task new_keys_should_use_the_short_initialization_cache_duration()
        {
            _options.KeyManagement.InitializationKeyCacheDuration = TimeSpan.FromMinutes(1);
            var cache = new InMemoryKeyStoreCache(_clock);

            await CreateSubject(cache).GetCurrentKeysAsync();
            var loads = _store.LoadCount;

            _clock.Advance(TimeSpan.FromMinutes(1));
            await CreateSubject(cache).GetCurrentKeysAsync();

            _store.LoadCount.Should().Be(loads + 1);
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData(14, 14, 1)]
        [InlineData(90, 14, 24 * 14)]
        public void invalid_timing_options_should_be_rejected(int rotationDays, int propagationDays, int cacheHours)
        {
            _options.KeyManagement.RotationInterval = TimeSpan.FromDays(rotationDays);
            _options.KeyManagement.PropagationTime = TimeSpan.FromDays(propagationDays);
            _options.KeyManagement.KeyCacheDuration = TimeSpan.FromHours(cacheHours);

            Action act = () => CreateSubject();

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        [Trait("Category", Category)]
        public void unsupported_algorithm_should_be_rejected()
        {
            _options.KeyManagement.SigningAlgorithms = new[] { SecurityAlgorithms.HmacSha256 };

            Action act = () => CreateSubject();

            act.Should().Throw<InvalidOperationException>().WithMessage("*HS256*");
        }
    }
}
