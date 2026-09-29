// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Duende.IdentityModel.Client;
using FluentAssertions;
using IdentityServer.IntegrationTests.Common;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using JsonWebKeySet = Microsoft.IdentityModel.Tokens.JsonWebKeySet;

namespace IdentityServer.IntegrationTests.Endpoints.Discovery
{
    public class AutomaticKeyManagementTests
    {
        private const string Category = "Automatic key management";

        private readonly InMemorySigningKeyStore _store = new InMemorySigningKeyStore();
        private readonly IdentityServerPipeline _pipeline = new IdentityServerPipeline();

        public AutomaticKeyManagementTests()
        {
            _pipeline.Clients.Add(new Client
            {
                ClientId = "client",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "api1" }
            });
            _pipeline.ApiScopes.Add(new ApiScope("api1"));
        }

        private void Initialize(params string[] algorithms)
        {
            _pipeline.OnPostConfigureServices += services =>
            {
                // only automatically managed keys, instead of the pipeline's developer signing credential
                services.RemoveAll<ISigningCredentialStore>();
                services.RemoveAll<IValidationKeysStore>();
                services.AddSingleton<ISigningKeyStore>(_store);

                services.Configure<IdentityServerOptions>(options =>
                {
                    options.KeyManagement.Enabled = true;
                    options.KeyManagement.InitializationSynchronizationDelay = TimeSpan.Zero;
                    if (algorithms.Length > 0) options.KeyManagement.SigningAlgorithms = algorithms;
                });
            };
            _pipeline.Initialize();
        }

        private async Task<JsonWebKeySet> GetKeySetAsync()
        {
            var json = await _pipeline.BackChannelClient.GetStringAsync(IdentityServerPipeline.DiscoveryKeysEndpoint);
            return new JsonWebKeySet(json);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task access_token_should_be_signed_with_a_managed_key_published_in_the_jwks()
        {
            Initialize();

            var response = await _pipeline.BackChannelClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });
            response.IsError.Should().BeFalse(response.Error);

            var keySet = await GetKeySetAsync();
            var result = await new JsonWebTokenHandler().ValidateTokenAsync(response.AccessToken, new TokenValidationParameters
            {
                ValidIssuer = "https://server",
                ValidateAudience = false,
                IssuerSigningKeys = keySet.GetSigningKeys()
            });

            result.IsValid.Should().BeTrue(result.Exception?.Message);
            var kid = new JsonWebToken(response.AccessToken).Kid;
            keySet.Keys.Should().ContainSingle().Which.Kid.Should().Be(kid);
            _store.Keys.Keys.Should().Equal(kid);
            keySet.Keys[0].HasPrivateKey.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task discovery_should_announce_all_managed_algorithms()
        {
            Initialize(SecurityAlgorithms.RsaSha256, SecurityAlgorithms.EcdsaSha256);

            var json = await _pipeline.BackChannelClient.GetStringAsync(IdentityServerPipeline.DiscoveryEndpoint);
            var algorithms = JsonNode.Parse(json)["id_token_signing_alg_values_supported"].AsArray().Select(x => x.GetValue<string>());

            algorithms.Should().BeEquivalentTo(new[] { SecurityAlgorithms.RsaSha256, SecurityAlgorithms.EcdsaSha256 });
            (await GetKeySetAsync()).Keys.Select(x => x.Kty).Should().BeEquivalentTo(new[] { "RSA", "EC" });
        }

        private class InMemorySigningKeyStore : ISigningKeyStore
        {
            public ConcurrentDictionary<string, SerializedKey> Keys { get; } = new ConcurrentDictionary<string, SerializedKey>();

            public Task<IEnumerable<SerializedKey>> LoadKeysAsync() => Task.FromResult<IEnumerable<SerializedKey>>(Keys.Values.ToList());

            public Task StoreKeyAsync(SerializedKey key)
            {
                Keys[key.Id] = key;
                return Task.CompletedTask;
            }

            public Task DeleteKeyAsync(string id)
            {
                Keys.TryRemove(id, out _);
                return Task.CompletedTask;
            }
        }
    }
}
