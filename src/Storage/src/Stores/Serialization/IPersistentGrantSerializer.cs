// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Stores.Serialization
{
    /// <summary>
    /// Interface for persisted grant serialization
    /// </summary>
    public interface IPersistentGrantSerializer
    {
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T">The persist grant type</typeparam>
        /// <param name="value">The persist grant value.</param>
        /// <returns>Returns serialized JSON string value of the persist grant.</returns>
        string Serialize<T>(T value);

        /// <summary>
        /// Deserializes the specified string.
        /// </summary>
        /// <typeparam name="T">The persist grant type</typeparam>
        /// <param name="json">The source JSON string value.</param>
        /// <returns>Returns deserialized instance of the persistent grant</returns>
        T? Deserialize<T>(string? json);
    }
}