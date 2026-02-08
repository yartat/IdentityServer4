// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#pragma warning disable 1591

namespace IdentityServer4.Models
{
    public class DiscoveryDocument
    {
#pragma warning disable IDE1006 // Naming Styles
        public required string issuer { get; set; }
        public required string jwks_uri { get; set; }
        public required string authorization_endpoint { get; set; }
        public required string token_endpoint { get; set; }
        public string? userinfo_endpoint { get; set; }
        public string? end_session_endpoint { get; set; }
        public string? check_session_iframe { get; set; }
        public string? revocation_endpoint { get; set; }
        public string? introspection_endpoint { get; set; }
        public bool? frontchannel_logout_supported { get; set; }
        public bool? frontchannel_logout_session_supported { get; set; }
        public required string[] scopes_supported { get; set; }
        public string[]? claims_supported { get; set; }
        public required string[] response_types_supported { get; set; }
        public required string[] response_modes_supported { get; set; }
        public required string[] grant_types_supported { get; set; }
        public required string[] subject_types_supported { get; set; }
        public required string[] id_token_signing_alg_values_supported { get; set; }
        public required string[] token_endpoint_auth_methods_supported { get; set; }
        public required string[] code_challenge_methods_supported { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}