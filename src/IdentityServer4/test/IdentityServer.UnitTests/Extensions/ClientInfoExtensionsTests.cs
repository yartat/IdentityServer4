// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Net;
using FluentAssertions;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace IdentityServer.UnitTests.Extensions
{
    public class ClientInfoExtensionsTests
    {
        private const string Category = "Client IP and device";

        private static HttpContext CreateContext(string remoteIp)
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = remoteIp == null ? null : IPAddress.Parse(remoteIp);
            return context;
        }

        [Fact]
        [Trait("Category", Category)]
        public void request_ip_should_ignore_headers_set_by_the_caller()
        {
            var context = CreateContext("10.0.0.5");
            context.Request.Headers["X-Forwarded-For"] = "1.2.3.4";
            context.Request.Headers["REMOTE_ADDR"] = "5.6.7.8";

            context.GetRequestIp().Should().Be("10.0.0.5");
        }

        [Fact]
        [Trait("Category", Category)]
        public void request_ip_without_connection_address_should_not_fall_back_to_headers()
        {
            var context = CreateContext(null);
            context.Request.Headers["REMOTE_ADDR"] = "5.6.7.8";

            context.GetRequestIp().Should().BeNull();
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData("1.2.3.4, 10.0.0.1", "1.2.3.4")]
        [InlineData("not-an-ip\r\ninjected", "10.0.0.5")]
        public void explicitly_requested_forwarded_header_should_only_be_used_when_it_is_an_ip_address(string header, string expected)
        {
            var context = CreateContext("10.0.0.5");
            context.Request.Headers["X-Forwarded-For"] = header;

            context.GetRequestIp(tryUseXForwardHeader: true).Should().Be(expected);
        }

        [Fact]
        [Trait("Category", Category)]
        public void device_should_keep_printable_ascii_only()
        {
            "Mozilla/5.0\r\n[ERR] forged log line\u0000 ünïcode".GetDevice()
                .Should().Be("Mozilla/5.0[ERR] forged log line ncode");
        }

        [Fact]
        [Trait("Category", Category)]
        public void device_should_be_truncated()
        {
            new string('a', 5000).GetDevice().Should().HaveLength(StringExtensions.MaxDeviceLength);
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\r\n\t")]
        public void empty_device_should_be_null(string userAgent)
        {
            userAgent.GetDevice().Should().BeNull();
        }
    }
}
