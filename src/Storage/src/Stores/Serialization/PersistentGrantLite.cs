// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace IdentityServer4.Storage.Stores.Serialization
{
    /// <summary>
    /// Define a persistent grant to serialize.
    /// </summary>
    [DataContract]
    public class PersistentGrantLite
    {
        /// <summary>
        /// A version number.
        /// </summary>
        [DataMember(Name = "version")]
        [JsonPropertyName("version")]
        public int Version { get; set; }

        /// <summary>
        /// A value indicating whether this object is protected.
        /// </summary>
        [DataMember(Name = "protected")]
        [JsonPropertyName("protected")]
        public bool IsProtected { get; set; }

        /// <summary>
        /// The data payload.
        /// </summary>
        [DataMember(Name = "payload")]
        [JsonPropertyName("payload")]
        public string? Payload { get; set; }
    }
}
