// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.UnitTests.Validation.Setup;
using Xunit;

namespace IdentityServer.UnitTests.Services.Default
{
    public class DefaultRefreshTokenServiceTests
    {
        private const string IpAddress = "192.168.0.1";
        private const string Device = "test";
        private DefaultRefreshTokenService _subject;
        private DefaultRefreshTokenStore _store;

        private ClaimsPrincipal _user = new IdentityServerUser("123").CreatePrincipal();
        private StubClock _clock = new StubClock();

        public DefaultRefreshTokenServiceTests()
        {
            _store = new DefaultRefreshTokenStore(
                new InMemoryPersistedGrantStore(),
                new PersistentGrantSerializer(),
                new DefaultHandleGenerationService(),
                TestLogger.Create<DefaultRefreshTokenStore>());

            _subject = new DefaultRefreshTokenService(
                _store, 
                new TestProfileService(),
                _clock, 
                TestLogger.Create<DefaultRefreshTokenService>());
        }

        [Fact]
        public async Task CreateRefreshToken_token_exists_in_store()
        {
            var client = new Client();
            var accessToken = new Token();

            var handle = await _subject.CreateRefreshTokenAsync(_user, accessToken, client, IpAddress, Device);

            (await _store.GetRefreshTokenAsync(handle)).Should().NotBeNull();
        }

        [Fact]
        public async Task CreateRefreshToken_should_match_absolute_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Absolute,
                AbsoluteRefreshTokenLifetime = 10
            };

            var handle = await _subject.CreateRefreshTokenAsync(_user, new Token(), client, IpAddress, Device);

            var refreshToken = (await _store.GetRefreshTokenAsync(handle));

            refreshToken.Should().NotBeNull();
            refreshToken.Lifetime.Should().Be(client.AbsoluteRefreshTokenLifetime);
        }

        [Fact]
        public async Task CreateRefreshToken_should_cap_sliding_lifetime_that_exceeds_absolute_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime  = 100,
                AbsoluteRefreshTokenLifetime = 10
            };

            var handle = await _subject.CreateRefreshTokenAsync(_user, new Token(), client, IpAddress, Device);

            var refreshToken = (await _store.GetRefreshTokenAsync(handle));

            refreshToken.Should().NotBeNull();
            refreshToken.Lifetime.Should().Be(client.AbsoluteRefreshTokenLifetime);
        }

        [Fact]
        public async Task CreateRefreshToken_should_match_sliding_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 10
            };

            var handle = await _subject.CreateRefreshTokenAsync(_user, new Token(), client, IpAddress, Device);

            var refreshToken = (await _store.GetRefreshTokenAsync(handle));

            refreshToken.Should().NotBeNull();
            refreshToken.Lifetime.Should().Be(client.SlidingRefreshTokenLifetime);
        }

        [Fact]
        public async Task UpdateRefreshToken_one_time_use_should_create_new_token()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.OneTimeOnly
            };

            var refreshToken = new RefreshToken
            {
                CreationTime = DateTime.UtcNow,
                Lifetime = 10,
                AccessToken = new Token
                {
                    ClientId = client.ClientId,
                    Audiences = { "aud" },
                    CreationTime = DateTime.UtcNow,
                    Claims = new List<Claim>()
                    {
                        new Claim("sub", "123")
                    }
                }
            };

            var handle = await _store.StoreRefreshTokenAsync(refreshToken);

            (await _subject.UpdateRefreshTokenAsync(handle, refreshToken, client))
                .Should().NotBeNull()
                .And
                .NotBe(handle);
        }

        private static RefreshToken CreateRefreshToken(string clientId, string subjectId) => new RefreshToken
        {
            CreationTime = DateTime.UtcNow,
            Lifetime = 3600,
            AccessToken = new Token
            {
                ClientId = clientId,
                Audiences = { "aud" },
                CreationTime = DateTime.UtcNow,
                Claims = new List<Claim> { new Claim("sub", subjectId) }
            }
        };

        [Fact]
        public async Task ValidateRefreshToken_reused_one_time_token_should_revoke_all_refresh_tokens_of_the_grant()
        {
            var client = new Client
            {
                ClientId = "client1",
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly
            };

            var stolen = await _store.StoreRefreshTokenAsync(CreateRefreshToken(client.ClientId, "123"));
            var otherDevice = await _store.StoreRefreshTokenAsync(CreateRefreshToken(client.ClientId, "123"));
            var otherClient = await _store.StoreRefreshTokenAsync(CreateRefreshToken("client2", "123"));
            var otherUser = await _store.StoreRefreshTokenAsync(CreateRefreshToken(client.ClientId, "456"));

            // the legitimate client uses the token and gets a successor
            var validation = await _subject.ValidateRefreshTokenAsync(stolen, client);
            validation.IsError.Should().BeFalse();
            var successor = await _subject.UpdateRefreshTokenAsync(stolen, validation.RefreshToken, client);

            // the attacker replays the consumed token
            (await _subject.ValidateRefreshTokenAsync(stolen, client)).IsError.Should().BeTrue();

            (await _store.GetRefreshTokenAsync(successor)).Should().BeNull();
            (await _store.GetRefreshTokenAsync(otherDevice)).Should().BeNull();
            (await _store.GetRefreshTokenAsync(otherClient)).Should().NotBeNull();
            (await _store.GetRefreshTokenAsync(otherUser)).Should().NotBeNull();
        }

        [Fact]
        public async Task ValidateRefreshToken_accepted_consumed_token_should_not_revoke()
        {
            var client = new Client
            {
                ClientId = "client1",
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly
            };
            var subject = new GraceWindowRefreshTokenService(_store, _clock);

            var handle = await _store.StoreRefreshTokenAsync(CreateRefreshToken(client.ClientId, "123"));
            var validation = await subject.ValidateRefreshTokenAsync(handle, client);
            var successor = await subject.UpdateRefreshTokenAsync(handle, validation.RefreshToken, client);

            (await subject.ValidateRefreshTokenAsync(handle, client)).IsError.Should().BeFalse();
            (await _store.GetRefreshTokenAsync(successor)).Should().NotBeNull();
        }

        private class GraceWindowRefreshTokenService : DefaultRefreshTokenService
        {
            public GraceWindowRefreshTokenService(IRefreshTokenStore store, StubClock clock)
                : base(store, new TestProfileService(), clock, TestLogger.Create<DefaultRefreshTokenService>())
            {
            }

            protected override Task<bool> AcceptConsumedTokenAsync(RefreshToken refreshToken) => Task.FromResult(true);
        }

        [Fact]
        public async Task UpdateRefreshToken_sliding_with_non_zero_absolute_should_update_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 10,
                AbsoluteRefreshTokenLifetime = 100
            };

            var now = DateTime.UtcNow;
            _clock.UtcNowFunc = () => now;

            var handle = await _store.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = now.AddSeconds(-10),
                AccessToken = new Token
                {
                    ClientId = client.ClientId,
                    Audiences = { "aud" },
                    CreationTime = DateTime.UtcNow,
                    Claims = new List<Claim>()
                    {
                        new Claim("sub", "123")
                    }
                }
            });

            var refreshToken = await _store.GetRefreshTokenAsync(handle);
            var newHandle = await _subject.UpdateRefreshTokenAsync(handle, refreshToken, client);

            newHandle.Should().NotBeNull().And.Be(handle);

            var newRefreshToken = await _store.GetRefreshTokenAsync(newHandle);

            newRefreshToken.Should().NotBeNull();
            newRefreshToken.Lifetime.Should().Be((int)(now - newRefreshToken.CreationTime).TotalSeconds + client.SlidingRefreshTokenLifetime);
        }

        [Fact]
        public async Task UpdateRefreshToken_lifetime_exceeds_absolute_should_be_absolute_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 10,
                AbsoluteRefreshTokenLifetime = 1000
            };

            var now = DateTime.UtcNow;
            _clock.UtcNowFunc = () => now;

            var handle = await _store.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = now.AddSeconds(-1000),
                AccessToken = new Token
                {
                    ClientId = client.ClientId,
                    Audiences = { "aud" },
                    CreationTime = DateTime.UtcNow,
                    Claims = new List<Claim>()
                    {
                        new Claim("sub", "123")
                    }
                }
            });

            var refreshToken = await _store.GetRefreshTokenAsync(handle);
            var newHandle = await _subject.UpdateRefreshTokenAsync(handle, refreshToken, client);

            newHandle.Should().NotBeNull().And.Be(handle);

            var newRefreshToken = await _store.GetRefreshTokenAsync(newHandle);

            newRefreshToken.Should().NotBeNull();
            newRefreshToken.Lifetime.Should().Be(client.AbsoluteRefreshTokenLifetime);
        }

        [Fact]
        public async Task UpdateRefreshToken_sliding_with_zero_absolute_should_update_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 10,
                AbsoluteRefreshTokenLifetime = 0
            };

            var now = DateTime.UtcNow;
            _clock.UtcNowFunc = () => now;

            var handle = await _store.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = now.AddSeconds(-1000),
                AccessToken = new Token
                {
                    ClientId = client.ClientId,
                    Audiences = { "aud" },
                    CreationTime = DateTime.UtcNow,
                    Claims = new List<Claim>()
                    {
                        new Claim("sub", "123")
                    }
                }
            });

            var refreshToken = await _store.GetRefreshTokenAsync(handle);
            var newHandle = await _subject.UpdateRefreshTokenAsync(handle, refreshToken, client);

            newHandle.Should().NotBeNull().And.Be(handle);

            var newRefreshToken = await _store.GetRefreshTokenAsync(newHandle);

            newRefreshToken.Should().NotBeNull();
            newRefreshToken.Lifetime.Should().Be((int)(now - newRefreshToken.CreationTime).TotalSeconds + client.SlidingRefreshTokenLifetime);
        }

        [Fact]
        public async Task UpdateRefreshToken_for_onetime_and_sliding_with_zero_absolute_should_update_lifetime()
        {
            var client = new Client
            {
                ClientId = "client1",
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 10,
                AbsoluteRefreshTokenLifetime = 0
            };

            var now = DateTime.UtcNow;
            _clock.UtcNowFunc = () => now;

            var handle = await _store.StoreRefreshTokenAsync(new RefreshToken
            {
                CreationTime = now.AddSeconds(-1000),
                AccessToken = new Token
                {
                    ClientId = client.ClientId,
                    Audiences = { "aud" },
                    CreationTime = DateTime.UtcNow,
                    Claims = new List<Claim>()
                    {
                        new Claim("sub", "123")
                    }
                }
            });

            var refreshToken = await _store.GetRefreshTokenAsync(handle);
            var newHandle = await _subject.UpdateRefreshTokenAsync(handle, refreshToken, client);

            newHandle.Should().NotBeNull().And.NotBe(handle);

            var newRefreshToken = await _store.GetRefreshTokenAsync(newHandle);

            newRefreshToken.Should().NotBeNull();
            newRefreshToken.Lifetime.Should().Be((int)(now - newRefreshToken.CreationTime).TotalSeconds + client.SlidingRefreshTokenLifetime);
        }
    }
}
