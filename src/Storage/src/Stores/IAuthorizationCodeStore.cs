// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for the authorization code store
    /// </summary>
    public interface IAuthorizationCodeStore
    {
        /// <summary>
        /// Stores the authorization code.
        /// </summary>
        /// <param name="code">The authorization code parameters.</param>
        /// <returns>Returns the authorization code identifier.</returns>
        Task<string> StoreAuthorizationCodeAsync(AuthorizationCode code);

        /// <summary>
        /// Stores the authorization code.
        /// </summary>
        /// <param name="handle">The authorization code identifier.</param>
        /// <param name="code">The authorization code parameters.</param>
        Task StoreAuthorizationCodeAsync(string handle, AuthorizationCode code);

        /// <summary>
        /// Gets the authorization code.
        /// </summary>
        /// <param name="handle">The authorization code identifier.</param>
        /// <returns>Returns the authorization code parameters.</returns>
        Task<AuthorizationCode> GetAuthorizationCodeAsync(string handle);

        /// <summary>
        /// Gets and removes the authorization code as atomic operation.
        /// </summary>
        /// <param name="handle">The authorization code identifier.</param>
        /// <returns>Returns the authorization code parameters.</returns>
        Task<AuthorizationCode> GetAndRemoveAuthorizationCodeAsync(string handle);

        /// <summary>
        /// Removes the authorization code.
        /// </summary>
        /// <param name="handle">The authorization code identifier.</param>
        Task RemoveAuthorizationCodeAsync(string handle);
   }
}