// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using Duende.IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;

namespace IdentityServer4.Logging
{
    internal class DeviceAuthorizationRequestValidationLog
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string Scopes { get; set; }

        public Dictionary<string, string> Raw { get; set; }

        private static readonly HashSet<string> SensitiveValuesFilter = new(StringComparer.OrdinalIgnoreCase)
        {
            OidcConstants.TokenRequest.ClientSecret,
            OidcConstants.TokenRequest.ClientAssertion
        };

        public DeviceAuthorizationRequestValidationLog(ValidatedDeviceAuthorizationRequest request)
        {
            Raw = request.Raw.ToScrubbedDictionary(SensitiveValuesFilter);

            if (request.Client != null)
            {
                ClientId = request.Client.ClientId;
                ClientName = request.Client.ClientName;
            }

            if (request.RequestedScopes != null)
            {
                Scopes = request.RequestedScopes.ToSpaceSeparatedString();
            }
        }

        public override string ToString()
        {
            return LogSerializer.Serialize(this);
        }
    }
}