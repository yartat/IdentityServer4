using System.Linq;

namespace IdentityServer4.EntityFramework.Mappers
{
    public partial class ClientMapper : IdentityServer4.EntityFramework.Mappers.IClientMapper
    {
        public IdentityServer4.Models.Client ToModel(IdentityServer4.EntityFramework.Entities.Client p1)
        {
            return p1 == null ? null : new IdentityServer4.Models.Client()
            {
                Enabled = p1.Enabled,
                ClientId = p1.ClientId,
                ProtocolType = p1.ProtocolType,
                ClientSecrets = p1.ClientSecrets == null ? new System.Collections.Generic.HashSet<IdentityServer4.Models.Secret>() : p1.ClientSecrets.Select<IdentityServer4.EntityFramework.Entities.ClientSecret, IdentityServer4.Models.Secret>(funcMain1).ToHashSet<IdentityServer4.Models.Secret>(),
                RequireClientSecret = p1.RequireClientSecret,
                ClientName = p1.ClientName,
                Description = p1.Description,
                ClientUri = p1.ClientUri,
                LogoUri = p1.LogoUri,
                RequireConsent = p1.RequireConsent,
                AllowRememberConsent = p1.AllowRememberConsent,
                AllowedGrantTypes = p1.AllowedGrantTypes == null ? new System.Collections.Generic.HashSet<string>() : p1.AllowedGrantTypes.Select<IdentityServer4.EntityFramework.Entities.ClientGrantType, string>(funcMain2).ToHashSet<string>(),
                RequirePkce = p1.RequirePkce,
                AllowPlainTextPkce = p1.AllowPlainTextPkce,
                RequireRequestObject = p1.RequireRequestObject,
                AllowAccessTokensViaBrowser = p1.AllowAccessTokensViaBrowser,
                RedirectUris = p1.RedirectUris == null ? new System.Collections.Generic.HashSet<System.Uri>() : p1.RedirectUris.Select<IdentityServer4.EntityFramework.Entities.ClientRedirectUri, System.Uri>(funcMain3).ToHashSet<System.Uri>(),
                PostLogoutRedirectUris = p1.PostLogoutRedirectUris == null ? new System.Collections.Generic.HashSet<System.Uri>() : p1.PostLogoutRedirectUris.Select<IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri, System.Uri>(funcMain4).ToHashSet<System.Uri>(),
                FrontChannelLogoutUri = p1.FrontChannelLogoutUri,
                FrontChannelLogoutSessionRequired = p1.FrontChannelLogoutSessionRequired,
                BackChannelLogoutUri = p1.BackChannelLogoutUri,
                BackChannelLogoutSessionRequired = p1.BackChannelLogoutSessionRequired,
                AllowOfflineAccess = p1.AllowOfflineAccess,
                AllowedScopes = p1.AllowedScopes == null ? new System.Collections.Generic.HashSet<string>() : p1.AllowedScopes.Select<IdentityServer4.EntityFramework.Entities.ClientScope, string>(funcMain5).ToHashSet<string>(),
                AlwaysIncludeUserClaimsInIdToken = p1.AlwaysIncludeUserClaimsInIdToken,
                IdentityTokenLifetime = p1.IdentityTokenLifetime,
                AllowedIdentityTokenSigningAlgorithms = string.IsNullOrWhiteSpace(p1.AllowedIdentityTokenSigningAlgorithms) ? new System.Collections.Generic.HashSet<string>() : p1.AllowedIdentityTokenSigningAlgorithms.Trim().Split(',', System.StringSplitOptions.RemoveEmptyEntries).ToHashSet<string>(),
                AccessTokenLifetime = p1.AccessTokenLifetime,
                AuthorizationCodeLifetime = p1.AuthorizationCodeLifetime,
                AbsoluteRefreshTokenLifetime = p1.AbsoluteRefreshTokenLifetime,
                SlidingRefreshTokenLifetime = p1.SlidingRefreshTokenLifetime,
                ConsentLifetime = p1.ConsentLifetime,
                RefreshTokenUsage = (IdentityServer4.Models.TokenUsage)p1.RefreshTokenUsage,
                UpdateAccessTokenClaimsOnRefresh = p1.UpdateAccessTokenClaimsOnRefresh,
                RefreshTokenExpiration = (IdentityServer4.Models.TokenExpiration)p1.RefreshTokenExpiration,
                AccessTokenType = (IdentityServer4.Models.AccessTokenType)p1.AccessTokenType,
                EnableLocalLogin = p1.EnableLocalLogin,
                IdentityProviderRestrictions = p1.IdentityProviderRestrictions == null ? new System.Collections.Generic.HashSet<string>() : p1.IdentityProviderRestrictions.Select<IdentityServer4.EntityFramework.Entities.ClientIdPRestriction, string>(funcMain6).ToHashSet<string>(),
                IncludeJwtId = p1.IncludeJwtId,
                Claims = p1.Claims == null ? new System.Collections.Generic.HashSet<IdentityServer4.Models.ClientClaim>() : p1.Claims.Select<IdentityServer4.EntityFramework.Entities.ClientClaim, IdentityServer4.Models.ClientClaim>(funcMain7).ToHashSet<IdentityServer4.Models.ClientClaim>(),
                AlwaysSendClientClaims = p1.AlwaysSendClientClaims,
                ClientClaimsPrefix = p1.ClientClaimsPrefix,
                PairWiseSubjectSalt = p1.PairWiseSubjectSalt,
                UserSsoLifetime = p1.UserSsoLifetime,
                UserCodeType = p1.UserCodeType,
                DeviceCodeLifetime = p1.DeviceCodeLifetime,
                AllowedCorsOrigins = p1.AllowedCorsOrigins == null ? new System.Collections.Generic.HashSet<string>() : p1.AllowedCorsOrigins.Select<IdentityServer4.EntityFramework.Entities.ClientCorsOrigin, string>(funcMain8).ToHashSet<string>(),
                Properties = p1.Properties == null ? new System.Collections.Generic.Dictionary<string, string>() : p1.Properties.ToDictionary<IdentityServer4.EntityFramework.Entities.ClientProperty, string, string>(funcMain9, funcMain10)
            };
        }
        public IdentityServer4.EntityFramework.Entities.Client ToEntity(IdentityServer4.Models.Client p2)
        {
            return p2 == null ? null : new IdentityServer4.EntityFramework.Entities.Client()
            {
                Enabled = p2.Enabled,
                ClientId = p2.ClientId,
                ProtocolType = p2.ProtocolType,
                ClientSecrets = p2.ClientSecrets == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ClientSecret>() : p2.ClientSecrets.Select<IdentityServer4.Models.Secret, IdentityServer4.EntityFramework.Entities.ClientSecret>(funcMain11).ToList<IdentityServer4.EntityFramework.Entities.ClientSecret>(),
                RequireClientSecret = p2.RequireClientSecret,
                ClientName = p2.ClientName,
                Description = p2.Description,
                ClientUri = p2.ClientUri,
                LogoUri = p2.LogoUri,
                RequireConsent = p2.RequireConsent,
                AllowRememberConsent = p2.AllowRememberConsent,
                AlwaysIncludeUserClaimsInIdToken = p2.AlwaysIncludeUserClaimsInIdToken,
                AllowedGrantTypes = p2.AllowedGrantTypes == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ClientGrantType>() : p2.AllowedGrantTypes.Select<string, IdentityServer4.EntityFramework.Entities.ClientGrantType>(funcMain12).ToList<IdentityServer4.EntityFramework.Entities.ClientGrantType>(),
                RequirePkce = p2.RequirePkce,
                AllowPlainTextPkce = p2.AllowPlainTextPkce,
                RequireRequestObject = p2.RequireRequestObject,
                AllowAccessTokensViaBrowser = p2.AllowAccessTokensViaBrowser,
                RedirectUris = p2.RedirectUris == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ClientRedirectUri>() : p2.RedirectUris.Select<System.Uri, IdentityServer4.EntityFramework.Entities.ClientRedirectUri>(funcMain13).ToList<IdentityServer4.EntityFramework.Entities.ClientRedirectUri>(),
                PostLogoutRedirectUris = p2.PostLogoutRedirectUris == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri>() : p2.PostLogoutRedirectUris.Select<System.Uri, IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri>(funcMain14).ToList<IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri>(),
                FrontChannelLogoutUri = p2.FrontChannelLogoutUri,
                FrontChannelLogoutSessionRequired = p2.FrontChannelLogoutSessionRequired,
                BackChannelLogoutUri = p2.BackChannelLogoutUri,
                BackChannelLogoutSessionRequired = p2.BackChannelLogoutSessionRequired,
                AllowOfflineAccess = p2.AllowOfflineAccess,
                AllowedScopes = p2.AllowedScopes == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ClientScope>() : p2.AllowedScopes.Select<string, IdentityServer4.EntityFramework.Entities.ClientScope>(funcMain15).ToList<IdentityServer4.EntityFramework.Entities.ClientScope>(),
                IdentityTokenLifetime = p2.IdentityTokenLifetime,
                AllowedIdentityTokenSigningAlgorithms = p2.AllowedIdentityTokenSigningAlgorithms == null || p2.AllowedIdentityTokenSigningAlgorithms.Count == 0 ? null : string.Join(",", p2.AllowedIdentityTokenSigningAlgorithms),
                AccessTokenLifetime = p2.AccessTokenLifetime,
                AuthorizationCodeLifetime = p2.AuthorizationCodeLifetime,
                ConsentLifetime = p2.ConsentLifetime,
                AbsoluteRefreshTokenLifetime = p2.AbsoluteRefreshTokenLifetime,
                SlidingRefreshTokenLifetime = p2.SlidingRefreshTokenLifetime,
                RefreshTokenUsage = (int)p2.RefreshTokenUsage,
                UpdateAccessTokenClaimsOnRefresh = p2.UpdateAccessTokenClaimsOnRefresh,
                RefreshTokenExpiration = (int)p2.RefreshTokenExpiration,
                AccessTokenType = (int)p2.AccessTokenType,
                EnableLocalLogin = p2.EnableLocalLogin,
                IdentityProviderRestrictions = p2.IdentityProviderRestrictions == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ClientIdPRestriction>() : p2.IdentityProviderRestrictions.Select<string, IdentityServer4.EntityFramework.Entities.ClientIdPRestriction>(funcMain16).ToList<IdentityServer4.EntityFramework.Entities.ClientIdPRestriction>(),
                IncludeJwtId = p2.IncludeJwtId,
                Claims = p2.Claims == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ClientClaim>() : p2.Claims.Select<IdentityServer4.Models.ClientClaim, IdentityServer4.EntityFramework.Entities.ClientClaim>(funcMain17).ToList<IdentityServer4.EntityFramework.Entities.ClientClaim>(),
                AlwaysSendClientClaims = p2.AlwaysSendClientClaims,
                ClientClaimsPrefix = p2.ClientClaimsPrefix,
                PairWiseSubjectSalt = p2.PairWiseSubjectSalt,
                AllowedCorsOrigins = p2.AllowedCorsOrigins == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ClientCorsOrigin>() : p2.AllowedCorsOrigins.Select<string, IdentityServer4.EntityFramework.Entities.ClientCorsOrigin>(funcMain18).ToList<IdentityServer4.EntityFramework.Entities.ClientCorsOrigin>(),
                Properties = p2.Properties == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ClientProperty>() : p2.Properties.Select<System.Collections.Generic.KeyValuePair<string, string>, IdentityServer4.EntityFramework.Entities.ClientProperty>(funcMain19).ToList<IdentityServer4.EntityFramework.Entities.ClientProperty>(),
                UserSsoLifetime = p2.UserSsoLifetime,
                UserCodeType = p2.UserCodeType,
                DeviceCodeLifetime = p2.DeviceCodeLifetime
            };
        }
        
        private IdentityServer4.Models.Secret funcMain1(IdentityServer4.EntityFramework.Entities.ClientSecret x)
        {
            return new IdentityServer4.Models.Secret()
            {
                Description = x.Description,
                Value = x.Value,
                Expiration = x.Expiration,
                Type = x.Type
            };
        }
        
        private string funcMain2(IdentityServer4.EntityFramework.Entities.ClientGrantType x)
        {
            return x.GrantType;
        }
        
        private System.Uri funcMain3(IdentityServer4.EntityFramework.Entities.ClientRedirectUri x)
        {
            return new System.Uri(x.RedirectUri, System.UriKind.RelativeOrAbsolute);
        }
        
        private System.Uri funcMain4(IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri x)
        {
            return new System.Uri(x.PostLogoutRedirectUri, System.UriKind.RelativeOrAbsolute);
        }
        
        private string funcMain5(IdentityServer4.EntityFramework.Entities.ClientScope x)
        {
            return x.Scope;
        }
        
        private string funcMain6(IdentityServer4.EntityFramework.Entities.ClientIdPRestriction x)
        {
            return x.Provider;
        }
        
        private IdentityServer4.Models.ClientClaim funcMain7(IdentityServer4.EntityFramework.Entities.ClientClaim x)
        {
            return new IdentityServer4.Models.ClientClaim(x.Type, x.Value, "http://www.w3.org/2001/XMLSchema#string");
        }
        
        private string funcMain8(IdentityServer4.EntityFramework.Entities.ClientCorsOrigin x)
        {
            return x.Origin;
        }
        
        private string funcMain9(IdentityServer4.EntityFramework.Entities.ClientProperty x)
        {
            return x.Key;
        }
        
        private string funcMain10(IdentityServer4.EntityFramework.Entities.ClientProperty x)
        {
            return x.Value;
        }
        
        private IdentityServer4.EntityFramework.Entities.ClientSecret funcMain11(IdentityServer4.Models.Secret x)
        {
            return new IdentityServer4.EntityFramework.Entities.ClientSecret()
            {
                Description = x.Description,
                Value = x.Value,
                Expiration = x.Expiration,
                Type = x.Type
            };
        }
        
        private IdentityServer4.EntityFramework.Entities.ClientGrantType funcMain12(string x)
        {
            return new IdentityServer4.EntityFramework.Entities.ClientGrantType() {GrantType = x};
        }
        
        private IdentityServer4.EntityFramework.Entities.ClientRedirectUri funcMain13(System.Uri x)
        {
            return new IdentityServer4.EntityFramework.Entities.ClientRedirectUri() {RedirectUri = x.OriginalString};
        }
        
        private IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri funcMain14(System.Uri x)
        {
            return new IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri() {PostLogoutRedirectUri = x.OriginalString};
        }
        
        private IdentityServer4.EntityFramework.Entities.ClientScope funcMain15(string x)
        {
            return new IdentityServer4.EntityFramework.Entities.ClientScope() {Scope = x};
        }
        
        private IdentityServer4.EntityFramework.Entities.ClientIdPRestriction funcMain16(string x)
        {
            return new IdentityServer4.EntityFramework.Entities.ClientIdPRestriction() {Provider = x};
        }
        
        private IdentityServer4.EntityFramework.Entities.ClientClaim funcMain17(IdentityServer4.Models.ClientClaim x)
        {
            return new IdentityServer4.EntityFramework.Entities.ClientClaim()
            {
                Type = x.Type,
                Value = x.Value
            };
        }
        
        private IdentityServer4.EntityFramework.Entities.ClientCorsOrigin funcMain18(string x)
        {
            return new IdentityServer4.EntityFramework.Entities.ClientCorsOrigin() {Origin = x};
        }
        
        private IdentityServer4.EntityFramework.Entities.ClientProperty funcMain19(System.Collections.Generic.KeyValuePair<string, string> x)
        {
            return new IdentityServer4.EntityFramework.Entities.ClientProperty()
            {
                Key = x.Key,
                Value = x.Value
            };
        }
    }
}