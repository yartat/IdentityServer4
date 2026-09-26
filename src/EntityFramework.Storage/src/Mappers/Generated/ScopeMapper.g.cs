using System.Linq;

namespace IdentityServer4.EntityFramework.Mappers
{
    public partial class ScopeMapper : IdentityServer4.EntityFramework.Mappers.IScopeMapper
    {
        public IdentityServer4.Models.ApiScope ToModel(IdentityServer4.EntityFramework.Entities.ApiScope p1)
        {
            return p1 == null ? null : new IdentityServer4.Models.ApiScope()
            {
                Required = p1.Required,
                Emphasize = p1.Emphasize,
                Enabled = p1.Enabled,
                Name = p1.Name,
                DisplayName = p1.DisplayName,
                Description = p1.Description,
                ShowInDiscoveryDocument = p1.ShowInDiscoveryDocument,
                UserClaims = p1.UserClaims == null ? new System.Collections.Generic.HashSet<string>() : p1.UserClaims.Select<IdentityServer4.EntityFramework.Entities.ApiScopeClaim, string>(funcMain1).ToHashSet<string>(),
                Properties = p1.Properties == null ? new System.Collections.Generic.Dictionary<string, string>() : p1.Properties.ToDictionary<IdentityServer4.EntityFramework.Entities.ApiScopeProperty, string, string>(funcMain2, funcMain3)
            };
        }
        public IdentityServer4.EntityFramework.Entities.ApiScope ToEntity(IdentityServer4.Models.ApiScope p2)
        {
            return p2 == null ? null : new IdentityServer4.EntityFramework.Entities.ApiScope()
            {
                Enabled = p2.Enabled,
                Name = p2.Name,
                DisplayName = p2.DisplayName,
                Description = p2.Description,
                Required = p2.Required,
                Emphasize = p2.Emphasize,
                ShowInDiscoveryDocument = p2.ShowInDiscoveryDocument,
                UserClaims = p2.UserClaims == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ApiScopeClaim>() : p2.UserClaims.Select<string, IdentityServer4.EntityFramework.Entities.ApiScopeClaim>(funcMain4).ToList<IdentityServer4.EntityFramework.Entities.ApiScopeClaim>(),
                Properties = p2.Properties == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.ApiScopeProperty>() : p2.Properties.Select<System.Collections.Generic.KeyValuePair<string, string>, IdentityServer4.EntityFramework.Entities.ApiScopeProperty>(funcMain5).ToList<IdentityServer4.EntityFramework.Entities.ApiScopeProperty>()
            };
        }
        
        private string funcMain1(IdentityServer4.EntityFramework.Entities.ApiScopeClaim x)
        {
            return x.Type;
        }
        
        private string funcMain2(IdentityServer4.EntityFramework.Entities.ApiScopeProperty x)
        {
            return x.Key;
        }
        
        private string funcMain3(IdentityServer4.EntityFramework.Entities.ApiScopeProperty x)
        {
            return x.Value;
        }
        
        private IdentityServer4.EntityFramework.Entities.ApiScopeClaim funcMain4(string x)
        {
            return new IdentityServer4.EntityFramework.Entities.ApiScopeClaim() {Type = x};
        }
        
        private IdentityServer4.EntityFramework.Entities.ApiScopeProperty funcMain5(System.Collections.Generic.KeyValuePair<string, string> x)
        {
            return new IdentityServer4.EntityFramework.Entities.ApiScopeProperty()
            {
                Key = x.Key,
                Value = x.Value
            };
        }
    }
}