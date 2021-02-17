// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Text.Json;

namespace IdentityServer4
{
    /// <summary>
    /// Defines an JSON object serializer extension methods.
    /// </summary>
    public static class ObjectSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };

        /// <summary>
        /// Returns a serialized to <see cref="string" /> that represents this instance.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns>A serialized to <see cref="string" /> that represents this instance.</returns>
        public static string ToString(object o)
        {
            return JsonSerializer.Serialize(o, Options);
        }

        /// <summary>
        /// Deserialize from the string.
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="value">The source string value.</param>
        /// <returns>Returns new instance of deserialized object.</returns>
        public static T FromString<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, Options);
        }
    }
}