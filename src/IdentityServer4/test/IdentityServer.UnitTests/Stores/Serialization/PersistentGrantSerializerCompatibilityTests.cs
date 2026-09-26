// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Security.Claims;
using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.Stores.Serialization;
using Xunit;

namespace IdentityServer.UnitTests.Stores.Serialization
{
    /// <summary>
    /// The fixtures were produced by the former Newtonsoft.Json based serializer. Grants stored in that format must stay
    /// readable, and the System.Text.Json output must be identical so earlier versions (rolling deploy, rollback) can read it.
    /// </summary>
    public class PersistentGrantSerializerCompatibilityTests
    {
        private const string Category = "PersistentGrantSerializer compatibility";

        private static readonly DateTime Created = new DateTime(2026, 9, 26, 10, 20, 30, DateTimeKind.Utc).AddTicks(1234567);

        private const string LegacyAuthorizationCode = """{"CreationTime":"2026-09-26T10:20:30.1234567Z","Lifetime":300,"ClientId":"client","Subject":{"AuthenticationType":"pwd","Claims":[{"Type":"sub","Value":"bob","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"name","Value":"Bob Smith","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"amr","Value":"pwd","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"auth_time","Value":"1790000000","ValueType":"http://www.w3.org/2001/XMLSchema#integer64"},{"Type":"address","Value":"{\"country\":\"UA\"}","ValueType":"json"}]},"IsOpenId":true,"RequestedScopes":["openid","api1"],"RedirectUri":"https://client/cb","Nonce":"n-1","StateHash":"sh","WasConsentShown":true,"SessionId":"session-1","CodeChallenge":"cc","CodeChallengeMethod":"S256","Description":"desc","Properties":{"p1":"v1"}}""";

        private const string LegacyRefreshToken = """{"CreationTime":"2026-09-26T10:20:30.1234567Z","Lifetime":2592000,"ConsumedTime":"2026-09-26T10:25:30.1234567Z","AccessToken":{"AllowedSigningAlgorithms":["RS256"],"Confirmation":"{\"x5t#S256\":\"abc\"}","Audiences":["api1","api2"],"Issuer":"https://id.example.com","CreationTime":"2026-09-26T10:20:30.1234567Z","Lifetime":3600,"Type":"access_token","ClientId":"client","AccessTokenType":1,"Description":"device A","Claims":[{"Type":"sub","Value":"bob","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"sid","Value":"session-1","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"scope","Value":"openid","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"scope","Value":"api1","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"client_id","Value":"client","ValueType":"http://www.w3.org/2001/XMLSchema#string"}],"Version":4},"IpAddress":"10.0.0.1","Device":"Mozilla/5.0","Version":4}""";

        private const string LegacyReferenceToken = """{"AllowedSigningAlgorithms":["RS256"],"Confirmation":"{\"x5t#S256\":\"abc\"}","Audiences":["api1","api2"],"Issuer":"https://id.example.com","CreationTime":"2026-09-26T10:20:30.1234567Z","Lifetime":3600,"Type":"access_token","ClientId":"client","AccessTokenType":1,"Description":"device A","Claims":[{"Type":"sub","Value":"bob","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"sid","Value":"session-1","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"scope","Value":"openid","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"scope","Value":"api1","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"client_id","Value":"client","ValueType":"http://www.w3.org/2001/XMLSchema#string"}],"Version":4}""";

        private const string LegacyUserConsent = """{"SubjectId":"bob","ClientId":"client","Scopes":["openid","api1"],"CreationTime":"2026-09-26T10:20:30.1234567Z","Expiration":"2026-10-26T10:20:30.1234567Z"}""";

        private const string LegacyDeviceCode = """{"CreationTime":"2026-09-26T10:20:30.1234567Z","Lifetime":300,"ClientId":"device","Description":"tv","IsOpenId":true,"IsAuthorized":true,"RequestedScopes":["openid","api1"],"AuthorizedScopes":["openid"],"Subject":{"AuthenticationType":"pwd","Claims":[{"Type":"sub","Value":"bob","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"name","Value":"Bob Smith","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"amr","Value":"pwd","ValueType":"http://www.w3.org/2001/XMLSchema#string"},{"Type":"auth_time","Value":"1790000000","ValueType":"http://www.w3.org/2001/XMLSchema#integer64"},{"Type":"address","Value":"{\"country\":\"UA\"}","ValueType":"json"}]},"SessionId":"session-1"}""";

        private const string LegacyDeviceCodeUnauthorized = """{"CreationTime":"2026-09-26T10:20:30.1234567Z","Lifetime":300,"ClientId":"device","Description":null,"IsOpenId":false,"IsAuthorized":false,"RequestedScopes":["openid"],"AuthorizedScopes":null,"Subject":null,"SessionId":null}""";

        private readonly PersistentGrantSerializer _subject = new PersistentGrantSerializer();

        [Theory]
        [Trait("Category", Category)]
        [InlineData(typeof(AuthorizationCode), LegacyAuthorizationCode)]
        [InlineData(typeof(RefreshToken), LegacyRefreshToken)]
        [InlineData(typeof(Token), LegacyReferenceToken)]
        [InlineData(typeof(Consent), LegacyUserConsent)]
        [InlineData(typeof(DeviceCode), LegacyDeviceCode)]
        [InlineData(typeof(DeviceCode), LegacyDeviceCodeUnauthorized)]
        public void legacy_json_should_round_trip_unchanged(Type grantType, string legacyJson)
        {
            var deserialize = typeof(PersistentGrantSerializer).GetMethod(nameof(PersistentGrantSerializer.Deserialize)).MakeGenericMethod(grantType);
            var serialize = typeof(PersistentGrantSerializer).GetMethod(nameof(PersistentGrantSerializer.Serialize)).MakeGenericMethod(grantType);

            var grant = deserialize.Invoke(_subject, new object[] { legacyJson });
            var json = (string)serialize.Invoke(_subject, new[] { grant });

            json.Should().Be(legacyJson);
        }

        [Fact]
        [Trait("Category", Category)]
        public void legacy_authorization_code_should_be_read()
        {
            var code = _subject.Deserialize<AuthorizationCode>(LegacyAuthorizationCode);

            code.CreationTime.Should().Be(Created);
            code.CreationTime.Kind.Should().Be(DateTimeKind.Utc);
            code.Lifetime.Should().Be(300);
            code.ClientId.Should().Be("client");
            code.IsOpenId.Should().BeTrue();
            code.RequestedScopes.Should().Equal("openid", "api1");
            code.RedirectUri.Should().Be("https://client/cb");
            code.CodeChallengeMethod.Should().Be("S256");
            code.Properties.Should().BeOfType<System.Collections.Generic.Dictionary<string, string>>()
                .Which.Should().ContainKey("p1").WhoseValue.Should().Be("v1");
            AssertBob(code.Subject);
        }

        [Fact]
        [Trait("Category", Category)]
        public void legacy_refresh_token_should_be_read()
        {
            var token = _subject.Deserialize<RefreshToken>(LegacyRefreshToken);

            token.CreationTime.Should().Be(Created);
            token.ConsumedTime.Should().Be(Created.AddMinutes(5));
            token.Lifetime.Should().Be(2592000);
            token.IpAddress.Should().Be("10.0.0.1");
            token.Device.Should().Be("Mozilla/5.0");
            token.ClientId.Should().Be("client");
            token.SubjectId.Should().Be("bob");
            token.SessionId.Should().Be("session-1");
            token.Scopes.Should().Equal("openid", "api1");
            token.AccessToken.AccessTokenType.Should().Be(AccessTokenType.Reference);
            token.AccessToken.Confirmation.Should().Be("{\"x5t#S256\":\"abc\"}");
        }

        [Fact]
        [Trait("Category", Category)]
        public void legacy_token_collections_should_keep_their_model_types()
        {
            var token = _subject.Deserialize<Token>(LegacyReferenceToken);

            // filled in place, as Newtonsoft.Json did: the claim set keeps its ClaimComparer and drops duplicates
            token.Claims.Should().BeOfType<System.Collections.Generic.HashSet<Claim>>();
            token.Claims.Add(new Claim("sub", "bob"));
            token.Claims.Count(c => c.Type == "sub").Should().Be(1);
            token.Audiences.Should().BeOfType<System.Collections.Generic.HashSet<string>>().And.BeEquivalentTo("api1", "api2");
            token.AllowedSigningAlgorithms.Should().BeEquivalentTo("RS256");
        }

        [Fact]
        [Trait("Category", Category)]
        public void legacy_consent_should_be_read()
        {
            var consent = _subject.Deserialize<Consent>(LegacyUserConsent);

            consent.SubjectId.Should().Be("bob");
            consent.Scopes.Should().Equal("openid", "api1");
            consent.Expiration.Should().Be(Created.AddDays(30));
        }

        [Fact]
        [Trait("Category", Category)]
        public void legacy_device_codes_should_be_read()
        {
            var authorized = _subject.Deserialize<DeviceCode>(LegacyDeviceCode);
            authorized.IsAuthorized.Should().BeTrue();
            authorized.AuthorizedScopes.Should().Equal("openid");
            AssertBob(authorized.Subject);

            var pending = _subject.Deserialize<DeviceCode>(LegacyDeviceCodeUnauthorized);
            pending.IsAuthorized.Should().BeFalse();
            pending.Subject.Should().BeNull();
            pending.AuthorizedScopes.Should().BeNull();
        }

        private static void AssertBob(ClaimsPrincipal subject)
        {
            subject.Identity.AuthenticationType.Should().Be("pwd");
            subject.Identity.Name.Should().Be("Bob Smith");
            subject.FindFirst("sub").Value.Should().Be("bob");
            subject.FindFirst("auth_time").ValueType.Should().Be(ClaimValueTypes.Integer64);
            subject.FindFirst("address").ValueType.Should().Be("json");
            subject.FindFirst("address").Value.Should().Be("{\"country\":\"UA\"}");
        }
    }
}
