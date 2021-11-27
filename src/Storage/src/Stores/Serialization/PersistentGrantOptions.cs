// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Storage.Stores.Serialization
{
    /// <summary>
    /// Defines an options to persist grants.
    /// </summary>
    public class PersistentGrantOptions
    {
        /// <summary>
        /// Indicates is data protected.
        /// </summary>
        public bool ProtectData { get; set; } = true;
    }
}
