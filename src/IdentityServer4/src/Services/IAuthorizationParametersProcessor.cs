// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Defines an interface for authorization parameters processor
    /// </summary>
    public interface IAuthorizationParametersProcessor
    {
        /// <summary>
        /// Stores the parameters asynchronous.
        /// </summary>
        /// <param name="request">The authorize request.</param>
        /// <returns>Returns identifier of the stored authorized parameters</returns>
        Task<(string ReturnUrl, string OtherParameters)> StoreParametersAsync(ValidatedAuthorizeRequest request);
    }
}
