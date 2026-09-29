// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Security.Claims;
using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services.KeyManagement;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace IdentityServer.UnitTests.Services.KeyManagement
{
    public class DataProtectionKeyProtectorTests
    {
        private const string Category = "DataProtectionKeyProtector";

        private readonly IdentityServerOptions _options = new IdentityServerOptions();
        private readonly IDataProtectionProvider _provider = new EphemeralDataProtectionProvider();

        [Theory]
        [Trait("Category", Category)]
        [InlineData(SecurityAlgorithms.RsaSha256)]
        [InlineData(SecurityAlgorithms.RsaSsaPssSha384)]
        [InlineData(SecurityAlgorithms.EcdsaSha256)]
        [InlineData(SecurityAlgorithms.EcdsaSha384)]
        [InlineData(SecurityAlgorithms.EcdsaSha512)]
        public void unprotected_key_should_verify_tokens_signed_with_the_original_key(string algorithm)
        {
            var subject = new DataProtectionKeyProtector(_provider, _options);
            var original = new KeyContainer("kid1", algorithm, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                KeyMaterial.Create(algorithm, "kid1", 2048));

            var serialized = subject.Protect(original);
            var restored = subject.Unprotect(serialized);

            restored.Id.Should().Be("kid1");
            restored.Algorithm.Should().Be(algorithm);
            restored.Created.Should().Be(original.Created);
            restored.Key.KeyId.Should().Be("kid1");

            // the restored key has the private key: it signs, and the original verifies
            var handler = new JsonWebTokenHandler();
            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", "1") }),
                SigningCredentials = restored.ToSigningCredentials()
            });
            var result = handler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                IssuerSigningKey = original.Key,
                ValidateIssuer = false,
                ValidateAudience = false
            }).Result;

            result.IsValid.Should().BeTrue(result.Exception?.Message);
        }

        [Fact]
        [Trait("Category", Category)]
        public void key_material_should_be_protected_by_default()
        {
            var subject = new DataProtectionKeyProtector(_provider, _options);

            var serialized = subject.Protect(new KeyContainer("kid1", SecurityAlgorithms.RsaSha256, DateTime.UtcNow, KeyMaterial.Create(SecurityAlgorithms.RsaSha256, "kid1", 2048)));

            serialized.Version.Should().Be(DataProtectionKeyProtector.CurrentVersion);
            serialized.DataProtected.Should().BeTrue();
            serialized.Data.Should().NotContain("\"d\"");
        }

        [Fact]
        [Trait("Category", Category)]
        public void key_material_should_be_stored_as_private_jwk_when_protection_is_disabled()
        {
            _options.KeyManagement.DataProtectKeys = false;
            var subject = new DataProtectionKeyProtector(_provider, _options);

            var serialized = subject.Protect(new KeyContainer("kid1", SecurityAlgorithms.EcdsaSha256, DateTime.UtcNow, KeyMaterial.Create(SecurityAlgorithms.EcdsaSha256, "kid1", 2048)));

            serialized.DataProtected.Should().BeFalse();
            serialized.Data.Should().Contain("\"kty\":\"EC\"").And.Contain("\"crv\":\"P-256\"").And.Contain("\"d\":").And.NotContain("null");
            subject.Unprotect(serialized).Key.Should().BeOfType<ECDsaSecurityKey>();
        }

        [Fact]
        [Trait("Category", Category)]
        public void unknown_version_should_be_rejected()
        {
            var subject = new DataProtectionKeyProtector(_provider, _options);
            var serialized = subject.Protect(new KeyContainer("kid1", SecurityAlgorithms.RsaSha256, DateTime.UtcNow, KeyMaterial.Create(SecurityAlgorithms.RsaSha256, "kid1", 2048)));
            serialized.Version = 99;

            Action act = () => subject.Unprotect(serialized);

            act.Should().Throw<InvalidOperationException>();
        }
    }
}
