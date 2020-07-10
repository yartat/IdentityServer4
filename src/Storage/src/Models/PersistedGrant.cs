// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace IdentityServer4.Models
{
    /// <summary>
    /// A model for a persisted grant
    /// </summary>
    [DataContract]
    public class PersistedGrant
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [DataMember(Name = "key")]
        [JsonPropertyName("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [DataMember(Name = "type")]
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <value>
        /// The subject identifier.
        /// </value>
        [DataMember(Name = "subjectId")]
        [JsonPropertyName("subjectId")]
        public string SubjectId { get; set; }

        /// <summary>
        /// Gets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        [DataMember(Name = "sessionId")]
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        [DataMember(Name = "clientId")]
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets the description the user assigned to the device being authorized.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember(Name = "description")]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        [DataMember(Name = "creationTime")]
        [JsonPropertyName("creationTime")]
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>
        /// The expiration.
        /// </value>
        [DataMember(Name = "expiration")]
        [JsonPropertyName("expiration")]
        public DateTime? Expiration { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [DataMember(Name = "data")]
        [JsonPropertyName("data")]
        public string Data { get; set; }
    }
}