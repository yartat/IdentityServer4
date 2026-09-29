// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.EntityFramework.IntegrationTests.Stores
{
    public class SigningKeyStoreTests : IntegrationTest<SigningKeyStoreTests, PersistedGrantDbContext, OperationalStoreOptions>
    {
        public SigningKeyStoreTests(DatabaseProviderFixture<PersistedGrantDbContext> fixture) : base(fixture)
        {
            foreach (var options in TestDatabaseProviders.Cast<object[]>().SelectMany(x => x.Select(y => (DbContextOptions<PersistedGrantDbContext>)y)).ToList())
            {
                using (var context = new PersistedGrantDbContext(options, StoreOptions))
                    context.Database.EnsureCreated();
            }
        }

        private static SerializedKey CreateKey() => new SerializedKey
        {
            Id = Guid.NewGuid().ToString("N"),
            Version = 1,
            Created = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Algorithm = "RS256",
            DataProtected = true,
            Data = new string('x', 5000)
        };

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task StoreKeyAsync_WhenKeyStored_ExpectKeyLoaded(DbContextOptions<PersistedGrantDbContext> options)
        {
            var key = CreateKey();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                await new SigningKeyStore(context, FakeLogger<SigningKeyStore>.Create()).StoreKeyAsync(key);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var keys = await new SigningKeyStore(context, FakeLogger<SigningKeyStore>.Create()).LoadKeysAsync();

                var loaded = keys.Single(x => x.Id == key.Id);
                loaded.Should().BeEquivalentTo(key, o => o.Excluding(x => x.Created));
                loaded.Created.Ticks.Should().Be(key.Created.Ticks);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task DeleteKeyAsync_WhenKeyExists_ExpectKeyDeleted(DbContextOptions<PersistedGrantDbContext> options)
        {
            var key = CreateKey();
            var other = CreateKey();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new SigningKeyStore(context, FakeLogger<SigningKeyStore>.Create());
                await store.StoreKeyAsync(key);
                await store.StoreKeyAsync(other);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                await new SigningKeyStore(context, FakeLogger<SigningKeyStore>.Create()).DeleteKeyAsync(key.Id);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.Keys.Any(x => x.Id == key.Id).Should().BeFalse();
                context.Keys.Any(x => x.Id == other.Id).Should().BeTrue();
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task DeleteKeyAsync_WhenKeyDoesNotExist_ExpectNoError(DbContextOptions<PersistedGrantDbContext> options)
        {
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                Func<Task> act = () => new SigningKeyStore(context, FakeLogger<SigningKeyStore>.Create()).DeleteKeyAsync(Guid.NewGuid().ToString("N"));

                await act.Should().NotThrowAsync();
            }
        }
    }
}
