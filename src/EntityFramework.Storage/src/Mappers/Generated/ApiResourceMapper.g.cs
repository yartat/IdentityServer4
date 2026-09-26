using System.Linq;

namespace IdentityServer4.EntityFramework.Mappers
{
    public partial class ApiResourceMapper : IdentityServer4.EntityFramework.Mappers.IApiResourceMapper
    {
        public IdentityServer4.Models.ApiResource ToModel(IdentityServer4.EntityFramework.Entities.ApiResource p1)
        {
            return p1 == null ? null : new IdentityServer4.Models.ApiResource()
            {
                ApiSecrets = p1.Secrets == null ? new System.Collections.Generic.HashSet<IdentityServer4.Models.Secret>() : p1.Secrets.Select<IdentityServer4.EntityFramework.Entities.ApiResourceSecret, IdentityServer4.Models.Secret>(funcMain1).ToHashSet<IdentityServer4.Models.Secret>(),
                Scopes = p1.Scopes == null ? new System.Collections.Generic.HashSet<string>() : p1.Scopes.Select<IdentityServer4.EntityFramework.Entities.ApiResourceScope, string>(funcMain2).ToHashSet<string>(),
                AllowedAccessTokenSigningAlgorithms = string.IsNullOrWhiteSpace(p1.AllowedAccessTokenSigningAlgorithms) ? new System.Collections.Generic.HashSet<string>() : p1.AllowedAccessTokenSigningAlgorithms.Trim().Split(',', System.StringSplitOptions.RemoveEmptyEntries).ToHashSet<string>(),
                Enabled = p1.Enabled,
                Name = p1.Name,
                DisplayName = p1.DisplayName,
                Description = p1.Description,
                ShowInDiscoveryDocument = p1.ShowInDiscoveryDocument,
                UserClaims = p1.UserClaims == null ? new System.Collections.Generic.HashSet<string>() : p1.UserClaims.Select<IdentityServer4.EntityFramework.Entities.ApiResourceClaim, string>(funcMain3).ToHashSet<string>(),
                Properties = p1.Properties == null ? new System.Collections.Generic.Dictionary<string, string>() : p1.Properties.ToDictionary<IdentityServer4.EntityFramework.Entities.ApiResourceProperty, string, string>(funcMain4, funcMain5)
            };
        }
        public IdentityServer4.EntityFramework.Entities.ApiResource ToEntity(IdentityServer4.Models.ApiResource p2)
        {
            return p2 == null ? null : new IdentityServer4.EntityFramework.Entities.ApiResource()
            {
                Enabled = p2.Enabled,
                Name = p2.Name,
                DisplayName = p2.DisplayName,
                Description = p2.Description,
                AllowedAccessTokenSigningAlgorithms = p2.AllowedAccessTokenSigningAlgorithms == null || p2.AllowedAccessTokenSigningAlgorithms.Count == 0 ? null : string.Join(",", p2.AllowedAccessTokenSigningAlgorithms),
                ShowInDiscoveryDocument = p2.ShowInDiscoveryDocument,
                Secrets = p2.ApiSecrets == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ApiResourceSecret>() : p2.ApiSecrets.Select<IdentityServer4.Models.Secret, IdentityServer4.EntityFramework.Entities.ApiResourceSecret>(funcMain6).ToList<IdentityServer4.EntityFramework.Entities.ApiResourceSecret>(),
                Scopes = p2.Scopes == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ApiResourceScope>() : p2.Scopes.Select<string, IdentityServer4.EntityFramework.Entities.ApiResourceScope>(funcMain7).ToList<IdentityServer4.EntityFramework.Entities.ApiResourceScope>(),
                UserClaims = p2.UserClaims == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ApiResourceClaim>() : p2.UserClaims.Select<string, IdentityServer4.EntityFramework.Entities.ApiResourceClaim>(funcMain8).ToList<IdentityServer4.EntityFramework.Entities.ApiResourceClaim>(),
                Properties = p2.Properties == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ApiResourceProperty>() : p2.Properties.Select<System.Collections.Generic.KeyValuePair<string, string>, IdentityServer4.EntityFramework.Entities.ApiResourceProperty>(funcMain9).ToList<IdentityServer4.EntityFramework.Entities.ApiResourceProperty>()
            };
        }
        
        private IdentityServer4.Models.Secret funcMain1(IdentityServer4.EntityFramework.Entities.ApiResourceSecret x)
        {
            return new IdentityServer4.Models.Secret()
            {
                Description = x.Description,
                Value = x.Value,
                Expiration = x.Expiration,
                Type = x.Type
            };
        }
        
        private string funcMain2(IdentityServer4.EntityFramework.Entities.ApiResourceScope x)
        {
            return x.Scope;
        }
        
        private string funcMain3(IdentityServer4.EntityFramework.Entities.ApiResourceClaim x)
        {
            return x.Type;
        }
        
        private string funcMain4(IdentityServer4.EntityFramework.Entities.ApiResourceProperty x)
        {
            return x.Key;
        }
        
        private string funcMain5(IdentityServer4.EntityFramework.Entities.ApiResourceProperty x)
        {
            return x.Value;
        }
        
        private IdentityServer4.EntityFramework.Entities.ApiResourceSecret funcMain6(IdentityServer4.Models.Secret x)
        {
            return new IdentityServer4.EntityFramework.Entities.ApiResourceSecret()
            {
                Description = x.Description,
                Value = x.Value,
                Expiration = x.Expiration,
                Type = x.Type
            };
        }
        
        private IdentityServer4.EntityFramework.Entities.ApiResourceScope funcMain7(string x)
        {
            return new IdentityServer4.EntityFramework.Entities.ApiResourceScope() {Scope = x};
        }
        
        private IdentityServer4.EntityFramework.Entities.ApiResourceClaim funcMain8(string x)
        {
            return new IdentityServer4.EntityFramework.Entities.ApiResourceClaim() {Type = x};
        }
        
        private IdentityServer4.EntityFramework.Entities.ApiResourceProperty funcMain9(System.Collections.Generic.KeyValuePair<string, string> x)
        {
            return new IdentityServer4.EntityFramework.Entities.ApiResourceProperty()
            {
                Key = x.Key,
                Value = x.Value
            };
        }
    }
}