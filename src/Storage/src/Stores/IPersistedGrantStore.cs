// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for persisting any type of grant.
    /// </summary>
    public interface IPersistedGrantStore
    {
        /// <summary>
        /// Stores the grant.
        /// </summary>
        /// <param name="grant">The grant.</param>
        Task StoreAsync(PersistedGrant grant);

        /// <summary>
        /// Gets the grant.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Returns persist grant</returns>
        Task<PersistedGrant> GetAsync(string key);

        /// <summary>
        /// Gets and removes the grant as atomic operation.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Returns persist grant</returns>
        Task<PersistedGrant> GetAndRemoveAsync(string key);

        /// <summary>
        /// Gets all grants based on the filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>Returns enumeration of the persist grants.</returns>
        Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter);

        /// <summary>
        /// Removes the grant by key.
        /// </summary>
        /// <param name="key">The key.</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes all grants based on the filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        Task RemoveAllAsync(PersistedGrantFilter filter);
    }
}