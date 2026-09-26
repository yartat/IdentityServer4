// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services.Default
{
    /// <summary>
    /// Defines a default implementation of the user profile validator.
    /// Implements the <see cref="IUserValidator" />
    /// </summary>
    /// <seealso cref="IUserValidator" />
    public class DefaultUserValidator : IUserValidator
    {
        private readonly IProfileService _profile;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUserValidator"/> class.
        /// </summary>
        /// <param name="profile">The profile service instance.</param>
        /// <exception cref="ArgumentNullException">profile</exception>
        public DefaultUserValidator(IProfileService profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        /// <inheritdoc/>
        public async Task<bool> IsActiveAsync(ClaimsPrincipal subject, Client client, string caller)
        {
            var isActiveCtx = new IsActiveContext(subject, client, caller);
            await _profile.IsActiveAsync(isActiveCtx);
            return isActiveCtx.IsActive;
        }
    }
}
