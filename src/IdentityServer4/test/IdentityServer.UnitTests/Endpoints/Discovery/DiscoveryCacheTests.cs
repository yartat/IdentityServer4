// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Xunit;

namespace IdentityServer.UnitTests.Endpoints.Discovery
{
    public class DiscoveryCacheTests
    {
        private const string Category = "Discovery cache";

        private readonly IdentityServerOptions _options = TestIdentityServerOptions.Create();
        private readonly TestClock _clock = new TestClock();
        private readonly CountingResponseGenerator _generator = new CountingResponseGenerator();
        private readonly MemoryCache _cache;

        public DiscoveryCacheTests()
        {
            _options.IssuerUri = null;
            _options.Discovery.CacheDuration = TimeSpan.FromMinutes(1);
            _cache = new MemoryCache(new MemoryCacheOptions { Clock = _clock });
        }

        private DiscoveryKeyEndpoint CreateKeyEndpoint() =>
            new DiscoveryKeyEndpoint(_options, _generator, _cache, TestLogger.Create<DiscoveryKeyEndpoint>());

        private DiscoveryEndpoint CreateDiscoveryEndpoint() =>
            new DiscoveryEndpoint(_options, _generator, _cache, TestLogger.Create<DiscoveryEndpoint>());

        private HttpContext CreateContext(string host = "server")
        {
            var context = new MockHttpContextAccessor(_options).HttpContext;
            context.Request.Method = "GET";
            context.Request.Scheme = "https";
            context.Request.Host = new HostString(host);
            return context;
        }

        private async Task<IEnumerable<JsonWebKey>> GetKeySetAsync() =>
            ((JsonWebKeysResult)await CreateKeyEndpoint().ProcessAsync(CreateContext())).WebKeys;

        [Fact]
        [Trait("Category", Category)]
        public async Task key_set_should_be_reused_within_cache_duration()
        {
            var first = await GetKeySetAsync();
            _clock.Advance(TimeSpan.FromSeconds(59));
            var second = await GetKeySetAsync();

            _generator.KeySetCalls.Should().Be(1);
            second.Should().BeSameAs(first);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task key_set_should_be_rebuilt_after_cache_duration()
        {
            var first = await GetKeySetAsync();
            _clock.Advance(TimeSpan.FromSeconds(61));
            var second = await GetKeySetAsync();

            _generator.KeySetCalls.Should().Be(2);
            second.Should().NotBeSameAs(first, "a rotated key must be published once the entry expires");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task zero_cache_duration_should_disable_caching()
        {
            _options.Discovery.CacheDuration = TimeSpan.Zero;

            await GetKeySetAsync();
            await GetKeySetAsync();
            await CreateDiscoveryEndpoint().ProcessAsync(CreateContext());
            await CreateDiscoveryEndpoint().ProcessAsync(CreateContext());

            _generator.KeySetCalls.Should().Be(2);
            _generator.DocumentCalls.Should().Be(2);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task discovery_document_should_be_cached_per_host()
        {
            var first = (DiscoveryDocumentResult)await CreateDiscoveryEndpoint().ProcessAsync(CreateContext("server1"));
            var second = (DiscoveryDocumentResult)await CreateDiscoveryEndpoint().ProcessAsync(CreateContext("server2"));
            var firstAgain = (DiscoveryDocumentResult)await CreateDiscoveryEndpoint().ProcessAsync(CreateContext("server1"));

            _generator.DocumentCalls.Should().Be(2);
            first.Entries["issuer"].Should().Be("https://server1");
            second.Entries["issuer"].Should().Be("https://server2");
            firstAgain.Entries.Should().BeSameAs(first.Entries);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task discovery_document_should_be_rebuilt_after_cache_duration()
        {
            await CreateDiscoveryEndpoint().ProcessAsync(CreateContext());
            _clock.Advance(TimeSpan.FromSeconds(61));
            await CreateDiscoveryEndpoint().ProcessAsync(CreateContext());

            _generator.DocumentCalls.Should().Be(2);
        }

        private class TestClock : ISystemClock
        {
            public DateTimeOffset UtcNow { get; private set; } = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            public void Advance(TimeSpan by) => UtcNow += by;
        }

        private class CountingResponseGenerator : IDiscoveryResponseGenerator
        {
            public int DocumentCalls { get; private set; }
            public int KeySetCalls { get; private set; }

            public Task<Dictionary<string, object>> CreateDiscoveryDocumentAsync(string baseUrl, string issuerUri)
            {
                DocumentCalls++;
                return Task.FromResult(new Dictionary<string, object> { { "issuer", issuerUri } });
            }

            public Task<IEnumerable<JsonWebKey>> CreateJwkDocumentAsync()
            {
                KeySetCalls++;
                return Task.FromResult<IEnumerable<JsonWebKey>>(new List<JsonWebKey> { new JsonWebKey { kid = "key" + KeySetCalls } });
            }
        }
    }
}
