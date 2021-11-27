// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Models the result of JWT request validation.
    /// </summary>
    public class JwtRequestValidationResult : ValidationResult
    {
        /// <summary>
        /// The list of the claims from the JWT payload of a successfully validated request.
        /// </summary>
        public IEnumerable<Claim> Payload { get; set; } = default!;
    }
}