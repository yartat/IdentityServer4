// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Mapster;

namespace IdentityServer4.EntityFramework.Mappers
{
    /// <summary>
    /// Mapster configuration for clients. Consumed by Mapster.Tool when generating <c>ClientMapper</c>.
    /// </summary>
    /// <remarks>
    /// Expressions use only BCL calls: Mapster.Tool turns calls to methods of this assembly into
    /// delegate fields that are never assigned. A null source collection maps to an empty collection.
    /// </remarks>
    internal sealed class ClientMapperRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Entities.Client, Models.Client>()
                .ShallowCopyForSameType(true)
                .Map(dest => dest.ClientSecrets, src => (ICollection<Models.Secret>)(src.ClientSecrets == null
                    ? new HashSet<Models.Secret>()
                    : src.ClientSecrets.Select(x => new Models.Secret
                    {
                        Description = x.Description,
                        Value = x.Value,
                        Expiration = x.Expiration,
                        Type = x.Type
                    }).ToHashSet()))
                .Map(dest => dest.AllowedGrantTypes, src => (ISet<string>)(src.AllowedGrantTypes == null
                    ? new HashSet<string>()
                    : src.AllowedGrantTypes.Select(x => x.GrantType).ToHashSet()))
                .Map(dest => dest.RedirectUris, src => (ICollection<Uri>)(src.RedirectUris == null
                    ? new HashSet<Uri>()
                    : src.RedirectUris.Select(x => new Uri(x.RedirectUri, UriKind.RelativeOrAbsolute)).ToHashSet()))
                .Map(dest => dest.PostLogoutRedirectUris, src => (ICollection<Uri>)(src.PostLogoutRedirectUris == null
                    ? new HashSet<Uri>()
                    : src.PostLogoutRedirectUris.Select(x => new Uri(x.PostLogoutRedirectUri, UriKind.RelativeOrAbsolute)).ToHashSet()))
                .Map(dest => dest.AllowedScopes, src => (ICollection<string>)(src.AllowedScopes == null
                    ? new HashSet<string>()
                    : src.AllowedScopes.Select(x => x.Scope).ToHashSet()))
                .Map(dest => dest.AllowedIdentityTokenSigningAlgorithms, src => (ICollection<string>)(string.IsNullOrWhiteSpace(src.AllowedIdentityTokenSigningAlgorithms)
                    ? new HashSet<string>()
                    : src.AllowedIdentityTokenSigningAlgorithms.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet()))
                .Map(dest => dest.IdentityProviderRestrictions, src => (ICollection<string>)(src.IdentityProviderRestrictions == null
                    ? new HashSet<string>()
                    : src.IdentityProviderRestrictions.Select(x => x.Provider).ToHashSet()))
                .Map(dest => dest.Claims, src => (ICollection<Models.ClientClaim>)(src.Claims == null
                    ? new HashSet<Models.ClientClaim>()
                    : src.Claims.Select(x => new Models.ClientClaim(x.Type, x.Value, ClaimValueTypes.String)).ToHashSet()))
                .Map(dest => dest.AllowedCorsOrigins, src => (ICollection<string>)(src.AllowedCorsOrigins == null
                    ? new HashSet<string>()
                    : src.AllowedCorsOrigins.Select(x => x.Origin).ToHashSet()))
                // duplicate keys throw, as they did with AutoMapper
                .Map(dest => dest.Properties, src => (IDictionary<string, string>)(src.Properties == null
                    ? new Dictionary<string, string>()
                    : src.Properties.ToDictionary(x => x.Key, x => x.Value)));

            config.NewConfig<Models.Client, Entities.Client>()
                .ShallowCopyForSameType(true)
                .Ignore(dest => dest.Id, dest => dest.Created, dest => dest.Updated, dest => dest.LastAccessed, dest => dest.NonEditable)
                .Map(dest => dest.ClientSecrets, src => src.ClientSecrets == null
                    ? new List<Entities.ClientSecret>()
                    : src.ClientSecrets.Select(x => new Entities.ClientSecret
                    {
                        Description = x.Description,
                        Value = x.Value,
                        Expiration = x.Expiration,
                        Type = x.Type
                    }).ToList())
                .Map(dest => dest.AllowedGrantTypes, src => src.AllowedGrantTypes == null
                    ? new List<Entities.ClientGrantType>()
                    : src.AllowedGrantTypes.Select(x => new Entities.ClientGrantType { GrantType = x }).ToList())
                .Map(dest => dest.RedirectUris, src => src.RedirectUris == null
                    ? new List<Entities.ClientRedirectUri>()
                    : src.RedirectUris.Select(x => new Entities.ClientRedirectUri { RedirectUri = x.OriginalString }).ToList())
                .Map(dest => dest.PostLogoutRedirectUris, src => src.PostLogoutRedirectUris == null
                    ? new List<Entities.ClientPostLogoutRedirectUri>()
                    : src.PostLogoutRedirectUris.Select(x => new Entities.ClientPostLogoutRedirectUri { PostLogoutRedirectUri = x.OriginalString }).ToList())
                .Map(dest => dest.AllowedScopes, src => src.AllowedScopes == null
                    ? new List<Entities.ClientScope>()
                    : src.AllowedScopes.Select(x => new Entities.ClientScope { Scope = x }).ToList())
                .Map(dest => dest.AllowedIdentityTokenSigningAlgorithms, src => src.AllowedIdentityTokenSigningAlgorithms == null || src.AllowedIdentityTokenSigningAlgorithms.Count == 0
                    ? null
                    : string.Join(",", src.AllowedIdentityTokenSigningAlgorithms))
                .Map(dest => dest.IdentityProviderRestrictions, src => src.IdentityProviderRestrictions == null
                    ? new List<Entities.ClientIdPRestriction>()
                    : src.IdentityProviderRestrictions.Select(x => new Entities.ClientIdPRestriction { Provider = x }).ToList())
                .Map(dest => dest.Claims, src => src.Claims == null
                    ? new List<Entities.ClientClaim>()
                    : src.Claims.Select(x => new Entities.ClientClaim { Type = x.Type, Value = x.Value }).ToList())
                .Map(dest => dest.AllowedCorsOrigins, src => src.AllowedCorsOrigins == null
                    ? new List<Entities.ClientCorsOrigin>()
                    : src.AllowedCorsOrigins.Select(x => new Entities.ClientCorsOrigin { Origin = x }).ToList())
                .Map(dest => dest.Properties, src => src.Properties == null
                    ? new List<Entities.ClientProperty>()
                    : src.Properties.Select(x => new Entities.ClientProperty { Key = x.Key, Value = x.Value }).ToList());
        }
    }
}
