// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Defines an interface to handle complete authorize request
    /// </summary>
    public interface IAuthorizeRequestCompleteHandler
    {
        /// <summary>
        /// Processes the complete authorize request asynchronous.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="context">HTTP context instance.</param>
        /// <param name="principal">The claims principal.</param>
        /// <returns>Returns response result.</returns>
        Task<IEndpointResult> ProcessCompleteAuthorizeRequestAsync(NameValueCollection parameters, HttpContext context, ClaimsPrincipal principal = null);
    }
}
