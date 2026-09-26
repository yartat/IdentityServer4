// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Models the result of JWT request validation.
    /// </summary>
    public class JwtRequestValidationResult : ValidationResult
    {
        /// <summary>
        /// The key/value pairs from the JWT payload of a successfuly validated request.
        /// </summary>
        public Dictionary<string, string> Payload { get; set; }
    }
}