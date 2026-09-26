// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Mapster;

namespace IdentityServer4.EntityFramework.Mappers
{
    /// <summary>
    /// Mapster configuration for API scopes. Consumed by Mapster.Tool when generating <c>ScopeMapper</c>.
    /// </summary>
    /// <remarks>See <see cref="ClientMapperRegister"/> for the conventions used here.</remarks>
    internal sealed class ScopeMapperRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Entities.ApiScope, Models.ApiScope>()
                .ShallowCopyForSameType(true)
                .Map(dest => dest.UserClaims, src => (ICollection<string>)(src.UserClaims == null
                    ? new HashSet<string>()
                    : src.UserClaims.Select(x => x.Type).ToHashSet()))
                .Map(dest => dest.Properties, src => (IDictionary<string, string>)(src.Properties == null
                    ? new Dictionary<string, string>()
                    : src.Properties.ToDictionary(x => x.Key, x => x.Value)));

            config.NewConfig<Models.ApiScope, Entities.ApiScope>()
                .ShallowCopyForSameType(true)
                .Ignore(dest => dest.Id)
                .Map(dest => dest.UserClaims, src => src.UserClaims == null
                    ? new List<Entities.ApiScopeClaim>()
                    : src.UserClaims.Select(x => new Entities.ApiScopeClaim { Type = x }).ToList())
                .Map(dest => dest.Properties, src => src.Properties == null
                    ? new List<Entities.ApiScopeProperty>()
                    : src.Properties.Select(x => new Entities.ApiScopeProperty { Key = x.Key, Value = x.Value }).ToList());
        }
    }
}
