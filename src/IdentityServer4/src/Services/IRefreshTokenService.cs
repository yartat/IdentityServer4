// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using IdentityServer4.Validation;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Implements refresh token creation and validation
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Validates a refresh token.
        /// </summary>
        /// <param name="token">The refresh token.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        Task<TokenValidationResult> ValidateRefreshTokenAsync(string token, Client client);

        /// <summary>
        /// Creates the refresh token.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="client">The client.</param>
        /// <param name="ip">The client IP address.</param>
        /// <param name="device">The client logged device.</param>
        /// <returns>
        /// The refresh token handle
        /// </returns>
        Task<string> CreateRefreshTokenAsync(ClaimsPrincipal subject, Token accessToken, Client client, string ip, string device);

        /// <summary>
        /// Updates the refresh token.
        /// </summary>
        /// <param name="handle">The refresh token identifier.</param>
        /// <param name="refreshToken">The refresh token parameters.</param>
        /// <param name="client">The OpenId client.</param>
        /// <returns>
        /// The refresh token handle
        /// </returns>
        Task<string> UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken, Client client);

        /// <summary>
        /// Refreshes the refresh token.
        /// </summary>
        /// <param name="handle">The refresh token identifier.</param>
        /// <param name="refreshToken">The refresh token parameters.</param>
        /// <param name="client">The OpenId client.</param>
        /// <returns>
        /// The refresh token handle
        /// </returns>
        Task<string> RefreshTokenAsync(string handle, RefreshToken refreshToken, Client client);

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <param name="refreshTokenHandle">The refresh token handle.</param>
        /// <returns>Returns refresh token in case token exists or <c>null</c></returns>
        Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle);

        /// <summary>
        /// Removes the refresh token.
        /// </summary>
        /// <param name="refreshTokenHandle">The refresh token handle.</param>
        Task RemoveRefreshTokenAsync(string refreshTokenHandle);
    }
}