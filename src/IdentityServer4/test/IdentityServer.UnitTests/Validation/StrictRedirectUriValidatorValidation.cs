// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Xunit;

namespace IdentityServer.UnitTests.Validation
{
    public class StrictRedirectUriValidatorValidation
    {
        private const string Category = "StrictRedirectUriValidator";

        private readonly Client _client = new Client
        {
            ClientId = "client",
            RedirectUris = { new Uri("https://app.example.com/callback"), new Uri("/signin-oidc", UriKind.Relative) },
            PostLogoutRedirectUris = { new Uri("https://app.example.com/signed-out") }
        };

        [Theory]
        [Trait("Category", Category)]
        [InlineData("https://app.example.com/callback")]
        [InlineData("HTTPS://APP.EXAMPLE.COM/CALLBACK")]
        public async Task registered_uri_should_be_valid(string requestedUri)
        {
            (await new StrictRedirectUriValidator().IsRedirectUriValidAsync(requestedUri, _client)).Should().BeTrue();
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData("https://app.example.com/callback#access_token=stolen")]
        [InlineData("https://app.example.com/callback#")]
        [InlineData("https://app.example.com/callback/")]
        [InlineData("https://app.example.com/callback?x=1")]
        [InlineData("https://app.example.com/./callback")]
        [InlineData("https://app.example.com:443/callback")]
        [InlineData("https://user@app.example.com/callback")]
        public async Task uri_that_is_not_exactly_the_registered_one_should_be_invalid(string requestedUri)
        {
            (await new StrictRedirectUriValidator().IsRedirectUriValidAsync(requestedUri, _client)).Should().BeFalse();
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("/callback")]
        [InlineData("http://")]
        [InlineData("http://app.example.com:99999/callback")]
        public async Task malformed_uri_should_be_invalid_and_not_throw(string requestedUri)
        {
            var subject = new StrictRedirectUriValidator();

            (await subject.IsRedirectUriValidAsync(requestedUri, _client)).Should().BeFalse();
            (await subject.IsPostLogoutRedirectUriValidAsync(requestedUri, _client)).Should().BeFalse();
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData("https://evil.example.com/signin-oidc")]
        [InlineData("https://app.example.com/signin-oidc")]
        public async Task relative_registered_uri_should_not_match_any_host_by_default(string requestedUri)
        {
            (await new StrictRedirectUriValidator(new IdentityServerOptions()).IsRedirectUriValidAsync(requestedUri, _client)).Should().BeFalse();
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData("https://app.example.com/signin-oidc", true)]
        [InlineData("https://APP.example.com/signin-oidc", true)]
        [InlineData("https://tenant.example.org:8443/signin-oidc", true)]
        [InlineData("https://evil.example.com/signin-oidc", false)]
        [InlineData("http://app.example.com/signin-oidc", false)]
        [InlineData("https://app.example.com/signin-oidc#x", false)]
        [InlineData("https://app.example.com/other", false)]
        public async Task relative_registered_uri_should_only_match_on_allowed_origins(string requestedUri, bool expected)
        {
            var options = new IdentityServerOptions();
            options.Validation.AllowedRelativeRedirectUriOrigins.Add("https://app.example.com");
            options.Validation.AllowedRelativeRedirectUriOrigins.Add("https://tenant.example.org:8443/");

            (await new StrictRedirectUriValidator(options).IsRedirectUriValidAsync(requestedUri, _client)).Should().Be(expected);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task post_logout_uri_should_be_checked_against_post_logout_uris_only()
        {
            var subject = new StrictRedirectUriValidator();

            (await subject.IsPostLogoutRedirectUriValidAsync("https://app.example.com/signed-out", _client)).Should().BeTrue();
            (await subject.IsPostLogoutRedirectUriValidAsync("https://app.example.com/callback", _client)).Should().BeFalse();
        }
    }
}
