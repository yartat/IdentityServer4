// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace IdentityServer4.Stores.Serialization
{
    /// <summary>
    /// JSON-based persisted grant serializer
    /// </summary>
    /// <remarks>
    /// Produces the same JSON as the former Newtonsoft.Json implementation, so grants stored by earlier
    /// versions stay readable and earlier versions can read grants stored by this one.
    /// </remarks>
    /// <seealso cref="IdentityServer4.Stores.Serialization.IPersistentGrantSerializer" />
    public class PersistentGrantSerializer : IPersistentGrantSerializer
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            // as with Newtonsoft.Json, names match case-insensitively; collections the models initialize themselves
            // are filled in place via [JsonObjectCreationHandling(Populate)]
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { ExcludePropertiesWithoutSetter } },
            // the stored JSON is never embedded in HTML, so escape only what JSON requires, as Newtonsoft.Json did
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
            {
                new ClaimConverter(),
                new ClaimsPrincipalConverter()
            }
        };

        // Counterpart of the former Newtonsoft.Json CustomContractResolver: properties without a public setter
        // (computed ones such as Token.Scopes or RefreshToken.Subject) are not part of the stored format.
        private static void ExcludePropertiesWithoutSetter(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

            for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
            {
                if (typeInfo.Properties[i].Set == null)
                {
                    typeInfo.Properties.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _options);
        }

        /// <summary>
        /// Deserializes the specified string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }
    }
}
