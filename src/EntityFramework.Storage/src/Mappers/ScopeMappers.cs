// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.EntityFramework.Entities;

namespace IdentityServer4.EntityFramework.Mappers
{
    /// <summary>
    /// Extension methods to map to/from entity/model for scopes.
    /// </summary>
    public static class ScopeMappers
    {
        /// <summary>
        /// Maps an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static Models.ApiScope ToModel(this ApiScope entity)
        {
            return entity == null ? null : GeneratedMappers.Scope.ToModel(entity);
        }

        /// <summary>
        /// Maps a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static ApiScope ToEntity(this Models.ApiScope model)
        {
            return model == null ? null : GeneratedMappers.Scope.ToEntity(model);
        }
    }
}