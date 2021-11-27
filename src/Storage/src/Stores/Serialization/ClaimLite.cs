// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace IdentityServer4.Stores.Serialization
{
    /// <summary>
    /// Defines a claim type to serialize.
    /// </summary>
    [DataContract]
    public class ClaimLite
    {
        /// <summary>
        /// A claim type.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonPropertyName("type")]
        [Required]
        public string Type { get; set; } = default!;

        /// <summary>
        /// A claim string value.
        /// </summary>
        [DataMember(Name = "value")]
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        /// <summary>
        /// A claim value type.
        /// </summary>
        [DataMember(Name = "valueType")]
        [JsonPropertyName("valueType")]
        public string? ValueType { get; set; }
    }
}