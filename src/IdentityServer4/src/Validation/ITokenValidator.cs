// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Interface for the token validator
    /// </summary>
    public interface ITokenValidator
    {
        /// <summary>
        /// Validates an access token.
        /// </summary>
        /// <param name="token">The access token.</param>
        /// <param name="expectedScope">The expected scope.</param>
        /// <returns>Returns token validation result.</returns>
        Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null);

        /// <summary>
        /// Validates a refresh token.
        /// </summary>
        /// <param name="token">The refresh token.</param>
        /// <param name="client">The client.</param>
        /// <returns>Returns token validation result.</returns>
        Task<TokenValidationResult> ValidateRefreshTokenAsync(string token, Client client = null);

        /// <summary>
        /// Validates an identity token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="validateLifetime">if set to <c>true</c> the lifetime gets validated. Otherwise not.</param>
        /// <returns>Returns token validation result.</returns>
        Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true);
    }
}