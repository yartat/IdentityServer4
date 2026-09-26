// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Mapster;

namespace IdentityServer4.EntityFramework.Mappers
{
    /// <summary>
    /// Mapster configuration for identity resources. Consumed by Mapster.Tool when generating <c>IdentityResourceMapper</c>.
    /// </summary>
    /// <remarks>See <see cref="ClientMapperRegister"/> for the conventions used here.</remarks>
    internal sealed class IdentityResourceMapperRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Entities.IdentityResource, Models.IdentityResource>()
                .ShallowCopyForSameType(true)
                .Map(dest => dest.UserClaims, src => (ICollection<string>)(src.UserClaims == null
                    ? new HashSet<string>()
                    : src.UserClaims.Select(x => x.Type).ToHashSet()))
                .Map(dest => dest.Properties, src => (IDictionary<string, string>)(src.Properties == null
                    ? new Dictionary<string, string>()
                    : src.Properties.ToDictionary(x => x.Key, x => x.Value)));

            config.NewConfig<Models.IdentityResource, Entities.IdentityResource>()
                .ShallowCopyForSameType(true)
                .Ignore(dest => dest.Id, dest => dest.Created, dest => dest.Updated, dest => dest.NonEditable)
                .Map(dest => dest.UserClaims, src => src.UserClaims == null
                    ? new List<Entities.IdentityResourceClaim>()
                    : src.UserClaims.Select(x => new Entities.IdentityResourceClaim { Type = x }).ToList())
                .Map(dest => dest.Properties, src => src.Properties == null
                    ? new List<Entities.IdentityResourceProperty>()
                    : src.Properties.Select(x => new Entities.IdentityResourceProperty { Key = x.Key, Value = x.Value }).ToList());
        }
    }
}
