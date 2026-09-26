// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Validation;
using Xunit;

namespace IdentityServer.UnitTests.Extensions
{
    public class ValidatedAuthorizeRequestExtensionsTests
    {
        [Fact]
        public void GetAcrValues_should_return_snapshot_of_values()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                Raw = new System.Collections.Specialized.NameValueCollection()
            };
            request.AuthenticationContextReferenceClasses.Add("a");
            request.AuthenticationContextReferenceClasses.Add("b");
            request.AuthenticationContextReferenceClasses.Add("c");

            var acrs = request.GetAcrValues();
            foreach(var acr in acrs)
            {
                request.RemoveAcrValue(acr);
            }
        }
    }
}
