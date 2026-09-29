// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// Converts signing keys to and from the form they are persisted in.
    /// </summary>
    public interface ISigningKeyProtector
    {
        /// <summary>
        /// Serializes and (optionally) protects the key.
        /// </summary>
        /// <param name="key">The key.</param>
        SerializedKey Protect(KeyContainer key);

        /// <summary>
        /// Unprotects and deserializes the key.
        /// </summary>
        /// <param name="key">The persisted key.</param>
        KeyContainer Unprotect(SerializedKey key);
    }
}
