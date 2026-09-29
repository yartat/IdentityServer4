// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4.Services.KeyManagement;
using IdentityServer4.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace IdentityServer.UnitTests.Services.KeyManagement
{
    public class DefaultKeyMaterialServiceTests
    {
        private const string Category = "DefaultKeyMaterialService with key management";

        private readonly TestSigningKeyStore _store = new TestSigningKeyStore();

        private ServiceProvider CreateProvider(bool enabled, Action<IIdentityServerBuilder> configure = null, params string[] algorithms)
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var builder = services.AddIdentityServer(options =>
            {
                options.KeyManagement.Enabled = enabled;
                options.KeyManagement.InitializationSynchronizationDelay = TimeSpan.Zero;
                options.KeyManagement.KeyPath = Path.Combine(Path.GetTempPath(), "is4-keys-" + Guid.NewGuid().ToString("N"));
                if (algorithms.Length > 0) options.KeyManagement.SigningAlgorithms = algorithms;
            });
            builder.Services.AddSingleton<ISigningKeyStore>(_store);
            configure?.Invoke(builder);

            return services.BuildServiceProvider();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task when_disabled_no_keys_should_be_created()
        {
            using var provider = CreateProvider(enabled: false);
            var subject = provider.GetRequiredService<IKeyMaterialService>();

            (await subject.GetSigningCredentialsAsync()).Should().BeNull();
            (await subject.GetAllSigningCredentialsAsync()).Should().BeEmpty();
            (await subject.GetValidationKeysAsync()).Should().BeEmpty();
            _store.LoadCount.Should().Be(0);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task when_enabled_managed_keys_should_sign_and_be_published()
        {
            using var provider = CreateProvider(enabled: true);
            var subject = provider.GetRequiredService<IKeyMaterialService>();

            var credential = await subject.GetSigningCredentialsAsync();
            var keys = (await subject.GetValidationKeysAsync()).ToList();

            credential.Algorithm.Should().Be(SecurityAlgorithms.RsaSha256);
            keys.Should().ContainSingle();
            keys[0].Key.KeyId.Should().Be(credential.Key.KeyId);
            _store.Keys.Should().ContainSingle();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task static_credential_should_take_precedence_and_both_should_be_published()
        {
            var staticKey = CryptoHelper.CreateRsaSecurityKey();
            using var provider = CreateProvider(enabled: true, builder => builder.AddSigningCredential(staticKey, SecurityAlgorithms.RsaSha256));
            var subject = provider.GetRequiredService<IKeyMaterialService>();

            (await subject.GetSigningCredentialsAsync()).Key.Should().BeSameAs(staticKey);
            (await subject.GetAllSigningCredentialsAsync()).Should().HaveCount(2);
            (await subject.GetValidationKeysAsync()).Should().HaveCount(2);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task allowed_algorithms_should_select_a_managed_key()
        {
            using var provider = CreateProvider(enabled: true, null, SecurityAlgorithms.RsaSha256, SecurityAlgorithms.EcdsaSha256);
            var subject = provider.GetRequiredService<IKeyMaterialService>();

            (await subject.GetSigningCredentialsAsync()).Algorithm.Should().Be(SecurityAlgorithms.RsaSha256);
            (await subject.GetSigningCredentialsAsync(new[] { SecurityAlgorithms.EcdsaSha256 })).Key.Should().BeOfType<ECDsaSecurityKey>();

            Func<Task> act = () => subject.GetSigningCredentialsAsync(new[] { SecurityAlgorithms.RsaSha512 });
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        [Trait("Category", Category)]
        public void default_store_should_be_the_file_system_store_and_be_replaceable()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddIdentityServer();
            using (var provider = services.BuildServiceProvider())
            {
                provider.GetRequiredService<ISigningKeyStore>().Should().BeOfType<FileSystemKeyStore>();
            }

            services.AddIdentityServerBuilder().AddSigningKeyStore<TestSigningKeyStore>();
            using (var provider = services.BuildServiceProvider())
            {
                provider.GetRequiredService<ISigningKeyStore>().Should().BeOfType<TestSigningKeyStore>();
            }
        }
    }
}
