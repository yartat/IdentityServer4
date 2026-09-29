// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Mapster;

namespace IdentityServer4.EntityFramework.Mappers
{
    /// <summary>
    /// Mapster configuration for API resources. Consumed by Mapster.Tool when generating <c>ApiResourceMapper</c>.
    /// </summary>
    /// <remarks>See <see cref="ClientMapperRegister"/> for the conventions used here.</remarks>
    internal sealed class ApiResourceMapperRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Entities.ApiResource, Models.ApiResource>()
                .ShallowCopyForSameType(true)
                .Map(dest => dest.ApiSecrets, src => (ICollection<Models.Secret>)(src.Secrets == null
                    ? new HashSet<Models.Secret>()
                    : src.Secrets.Select(x => new Models.Secret
                    {
                        Description = x.Description,
                        Value = x.Value,
                        Expiration = x.Expiration,
                        Type = x.Type
                    }).ToHashSet()))
                .Map(dest => dest.Scopes, src => (ICollection<string>)(src.Scopes == null
                    ? new HashSet<string>()
                    : src.Scopes.Select(x => x.Scope).ToHashSet()))
                .Map(dest => dest.UserClaims, src => (ICollection<string>)(src.UserClaims == null
                    ? new HashSet<string>()
                    : src.UserClaims.Select(x => x.Type).ToHashSet()))
                .Map(dest => dest.AllowedAccessTokenSigningAlgorithms, src => (ICollection<string>)(string.IsNullOrWhiteSpace(src.AllowedAccessTokenSigningAlgorithms)
                    ? new HashSet<string>()
                    : src.AllowedAccessTokenSigningAlgorithms.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet()))
                .Map(dest => dest.Properties, src => (IDictionary<string, string>)(src.Properties == null
                    ? new Dictionary<string, string>()
                    : src.Properties.ToDictionary(x => x.Key, x => x.Value)));

            config.NewConfig<Models.ApiResource, Entities.ApiResource>()
                .ShallowCopyForSameType(true)
                .Ignore(dest => dest.Id, dest => dest.Created, dest => dest.Updated, dest => dest.LastAccessed, dest => dest.NonEditable)
                .Map(dest => dest.Secrets, src => src.ApiSecrets == null
                    ? new List<Entities.ApiResourceSecret>()
                    : src.ApiSecrets.Select(x => new Entities.ApiResourceSecret
                    {
                        Description = x.Description,
                        Value = x.Value,
                        Expiration = x.Expiration,
                        Type = x.Type
                    }).ToList())
                .Map(dest => dest.Scopes, src => src.Scopes == null
                    ? new List<Entities.ApiResourceScope>()
                    : src.Scopes.Select(x => new Entities.ApiResourceScope { Scope = x }).ToList())
                .Map(dest => dest.UserClaims, src => src.UserClaims == null
                    ? new List<Entities.ApiResourceClaim>()
                    : src.UserClaims.Select(x => new Entities.ApiResourceClaim { Type = x }).ToList())
                .Map(dest => dest.AllowedAccessTokenSigningAlgorithms, src => src.AllowedAccessTokenSigningAlgorithms == null || src.AllowedAccessTokenSigningAlgorithms.Count == 0
                    ? null
                    : string.Join(",", src.AllowedAccessTokenSigningAlgorithms))
                .Map(dest => dest.Properties, src => src.Properties == null
                    ? new List<Entities.ApiResourceProperty>()
                    : src.Properties.Select(x => new Entities.ApiResourceProperty { Key = x.Key, Value = x.Value }).ToList());
        }
    }
}
