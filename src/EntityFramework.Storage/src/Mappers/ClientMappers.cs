// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.EntityFramework.Mappers
{
    /// <summary>
    /// Extension methods to map to/from entity/model for clients.
    /// </summary>
    public static class ClientMappers
    {
        /// <summary>
        /// Maps an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static Models.Client ToModel(this Entities.Client entity)
        {
            return GeneratedMappers.Client.ToModel(entity);
        }

        /// <summary>
        /// Maps a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static Entities.Client ToEntity(this Models.Client model)
        {
            return GeneratedMappers.Client.ToEntity(model);
        }
    }
}