// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validates requested resources (scopes and resource indicators)
    /// </summary>
    public interface IResourceValidator
    {
        // todo: should this be used anywhere we re-create tokens? do we need to re-run scope validation?

        /// <summary>
        /// Validates the requested resources for the client.
        /// </summary>
        Task<ResourceValidationResult> ValidateRequestedResourcesAsync(ResourceValidationRequest request);
    }
}
