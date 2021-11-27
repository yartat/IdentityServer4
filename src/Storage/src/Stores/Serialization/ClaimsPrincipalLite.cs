// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace IdentityServer4.Stores.Serialization
{
    /// <summary>
    /// Defines a claims principal to serialize.
    /// </summary>
    [DataContract]
    public class ClaimsPrincipalLite
    {
        /// <summary>
        /// A type of the authentication.
        /// </summary>
        [DataMember(Name = "authenticationType")]
        [JsonPropertyName("authenticationType")]
        public string? AuthenticationType { get; set; }

        /// <summary>
        /// A principal claims.
        /// </summary>
        [DataMember(Name = "claims")]
        [JsonPropertyName("claims")]
        public ClaimLite[]? Claims { get; set; }
    }
}
