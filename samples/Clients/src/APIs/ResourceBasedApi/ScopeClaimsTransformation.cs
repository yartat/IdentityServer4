// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace ResourceBasedApi
{
    /// <summary>
    /// Splits a space-delimited scope claim (as returned by token introspection) into one claim per scope,
    /// so JWT and reference tokens are authorized the same way.
    /// </summary>
    internal sealed class ScopeClaimsTransformation : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var scopeClaims = principal.FindAll("scope").ToList();
            if (scopeClaims.Count != 1 || !scopeClaims[0].Value.Contains(' ') || principal.Identity is not ClaimsIdentity identity)
            {
                return Task.FromResult(principal);
            }

            var scopeClaim = scopeClaims[0];
            identity.RemoveClaim(scopeClaim);
            foreach (var scope in scopeClaim.Value.Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
            {
                identity.AddClaim(new Claim("scope", scope, scopeClaim.ValueType, scopeClaim.Issuer));
            }

            return Task.FromResult(principal);
        }
    }
}
