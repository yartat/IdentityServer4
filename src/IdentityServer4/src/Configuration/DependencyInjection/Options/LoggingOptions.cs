// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Duende.IdentityModel;
using System;
using System.Collections.Generic;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Options for configuring logging behavior
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// The token request sensitive values (to hide) filter
        /// </summary>
        public HashSet<string> TokenRequestSensitiveValuesFilter { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            OidcConstants.TokenRequest.ClientSecret,
            OidcConstants.TokenRequest.Password,
            OidcConstants.TokenRequest.ClientAssertion,
            OidcConstants.TokenRequest.RefreshToken,
            OidcConstants.TokenRequest.DeviceCode,
            OidcConstants.TokenRequest.Code,
            OidcConstants.TokenRequest.CodeVerifier
        };

        /// <summary>
        /// The authorize request sensitive values (to hide) filter
        /// </summary>
        public HashSet<string> AuthorizeRequestSensitiveValuesFilter { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            OidcConstants.AuthorizeRequest.IdTokenHint
        };
    }
}