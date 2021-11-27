// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using IdentityServer4.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text.Json;

namespace IdentityServer4.Extensions
{
    /// <summary>
    /// Extensions for Token
    /// </summary>
    public static class TokenExtensions
    {
        /// <summary>
        /// Gets the date time value from claim.
        /// </summary>
        /// <param name="claim">The claim value.</param>
        /// <returns>Returns <see cref="DateTime"/> value or <c>null</c>.</returns>
        public static DateTime? GetDateTime(this Claim? claim)
        {
            if (claim == null)
            {
                return null;
            }

            return EpochTime.DateTime(Convert.ToInt64(Math.Truncate(Convert.ToDouble(claim.Value, CultureInfo.InvariantCulture))));
        }

        /// <summary>
        /// Creates the default JWT payload.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="options">The options</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// </exception>
        public static JwtPayload CreateJwtPayload(this Token token, ISystemClock clock, IdentityServerOptions options, ILogger logger)
        {
            var issuedAtClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.IssuedAt).ToArray();
            var payload = new JwtPayload(
                token.Issuer,
                null,
                null,
                clock.UtcNow.UtcDateTime,
                clock.UtcNow.UtcDateTime.AddSeconds(token.Lifetime),
                issuedAtClaims.FirstOrDefault().GetDateTime());

            try
            {
                foreach (var aud in token.Audiences)
                {
                    payload.AddClaim(new Claim(JwtClaimTypes.Audience, aud));
                }

                var amrClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.AuthenticationMethod).ToArray();
                var scopeClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.Scope).ToArray();
                var jsonClaims = token.Claims.Where(x => x.ValueType == IdentityServerConstants.ClaimValueTypes.Json).ToList();

                // add confirmation claim if present (it's JSON valued)
                if (token.Confirmation.IsPresent())
                {
                    payload.Add(JwtClaimTypes.Confirmation,
                        JsonSerializer.Deserialize<JsonElement>(token.Confirmation!));
                }

                var normalClaims = token.Claims
                    .Except(amrClaims)
                    .Except(jsonClaims)
                    .Except(scopeClaims)
                    .Except(issuedAtClaims);

                payload.AddClaims(normalClaims);

                // scope claims
                if (!scopeClaims.IsNullOrEmpty())
                {
                    var scopeValues = scopeClaims.Select(x => x.Value).ToArray();

                    if (options.EmitScopesAsSpaceDelimitedStringInJwt)
                    {
                        payload.Add(JwtClaimTypes.Scope, string.Join(" ", scopeValues));
                    }
                    else
                    {
                        payload.Add(JwtClaimTypes.Scope, scopeValues);
                    }
                }

                // AMR claims
                if (!amrClaims.IsNullOrEmpty())
                {
                    var amrValues = amrClaims.Select(x => x.Value).Distinct().ToArray();
                    payload.Add(JwtClaimTypes.AuthenticationMethod, amrValues);
                }

                // other claims
                var otherClaimTypes = token.Claims
                    .Where(c => c.Type != JwtClaimTypes.AuthenticationMethod && c.Type != JwtClaimTypes.Scope)
                    .Select(c => c.Type)
                    .Distinct();

                foreach (var claimType in otherClaimTypes)
                {
                    var claims = token.Claims.Where(c => c.Type == claimType).ToArray();
                    if (claims.Length > 1)
                    {
                        payload.Add(claimType, AddObjects(claims));
                    }
                    else
                    {
                        payload.Add(claimType, AddObject(claims.First()));
                    }
                }

                return payload;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error creating a JSON valued claim");
                throw;
            }
        }

        private static IEnumerable<object> AddObjects(IEnumerable<Claim> claims)
        {
            foreach (var claim in claims)
            {
                var obj = AddObject(claim);
                if (obj is not null)
                {
                    yield return obj;
                }
            }
        }

        private static object? AddObject(Claim claim) =>
            claim.ValueType switch
            {
                ClaimValueTypes.Boolean => bool.TryParse(claim.Value, out var result) ? result : null,
                ClaimValueTypes.DateTime or ClaimValueTypes.Date => DateTime.TryParse(claim.Value, out var result) ? result : null,
                ClaimValueTypes.DaytimeDuration or ClaimValueTypes.Time => TimeSpan.TryParse(claim.Value, out var result) ? result : null,
                ClaimValueTypes.Double => double.TryParse(claim.Value, out var result) ? result : null,
                ClaimValueTypes.Integer or ClaimValueTypes.Integer32 => int.TryParse(claim.Value, out var result) ? result : null,
                ClaimValueTypes.Integer64 => long.TryParse(claim.Value, out var result) ? result : null,
                IdentityServerConstants.ClaimValueTypes.Json => JsonSerializer.Deserialize<JsonElement>(claim.Value),
                _ => claim.Value
            };
    }
}