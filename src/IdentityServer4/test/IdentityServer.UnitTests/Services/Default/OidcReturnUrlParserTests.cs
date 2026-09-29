// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer.UnitTests.Endpoints.Authorize;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Xunit;

namespace IdentityServer.UnitTests.Services.Default
{
    public class OidcReturnUrlParserTests
    {
        private const string Category = "OidcReturnUrlParser";

        private static OidcReturnUrlParser CreateParser(string baseUri = "https://id.example.com", params string[] allowedOrigins)
        {
            var options = new IdentityServerOptions { BaseUri = baseUri };
            foreach (var origin in allowedOrigins)
            {
                options.UserInteraction.AllowedReturnUrlOrigins.Add(origin);
            }

            return new OidcReturnUrlParser(
                new StubAuthorizeRequestValidator(),
                new MockUserSession(),
                options,
                TestLogger.Create<OidcReturnUrlParser>());
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData(null)]
        [InlineData("")]
        public void missing_return_url_should_be_invalid(string returnUrl)
        {
            CreateParser().IsValidReturnUrl(returnUrl).Should().BeFalse();
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData("/connect/authorize?client_id=client")]
        [InlineData("/connect/authorize/callback?client_id=client")]
        [InlineData("/identity/connect/authorize")]
        [InlineData("~/connect/authorize")]
        public void local_authorize_url_should_be_valid(string returnUrl)
        {
            CreateParser().IsValidReturnUrl(returnUrl).Should().BeTrue();
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData("/account/login")]
        [InlineData("/connect/token")]
        public void local_url_to_other_endpoint_should_be_invalid(string returnUrl)
        {
            CreateParser().IsValidReturnUrl(returnUrl).Should().BeFalse();
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData("https://id.example.com/connect/authorize/callback?client_id=client")]
        [InlineData("https://ID.EXAMPLE.COM/connect/authorize")]
        [InlineData("https://id.example.com:443/connect/authorize")]
        [InlineData("https://login.example.com:8443/connect/authorize?client_id=client")]
        public void absolute_url_on_base_uri_or_allowed_origin_should_be_valid(string returnUrl)
        {
            CreateParser("https://id.example.com/identity", "https://login.example.com:8443").IsValidReturnUrl(returnUrl).Should().BeTrue();
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData("https://evil.example/connect/authorize?client_id=client")]
        [InlineData("https://id.example.com.evil.example/connect/authorize")]
        [InlineData("https://id.example.com@evil.example/connect/authorize")]
        [InlineData("//evil.example/connect/authorize")]
        [InlineData("/\\evil.example/connect/authorize")]
        [InlineData("http://id.example.com/connect/authorize")]
        [InlineData("https://login.example.com/connect/authorize")]
        [InlineData("javascript:alert(1)//connect/authorize")]
        public void absolute_url_on_other_origin_should_be_invalid(string returnUrl)
        {
            CreateParser("https://id.example.com", "https://login.example.com:8443").IsValidReturnUrl(returnUrl).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void absolute_url_on_allowed_origin_to_other_endpoint_should_be_invalid()
        {
            CreateParser().IsValidReturnUrl("https://id.example.com/account/login").Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void absolute_url_should_be_invalid_when_no_origins_are_configured()
        {
            CreateParser(baseUri: null).IsValidReturnUrl("https://id.example.com/connect/authorize").Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void allowed_origin_entries_that_are_not_absolute_urls_should_be_ignored()
        {
            var parser = CreateParser(null, "login.example.com", "not a url");

            parser.IsValidReturnUrl("https://login.example.com/connect/authorize").Should().BeFalse();
        }
    }
}
