using System.Linq;

namespace IdentityServer4.EntityFramework.Mappers
{
    public partial class IdentityResourceMapper : IdentityServer4.EntityFramework.Mappers.IIdentityResourceMapper
    {
        public IdentityServer4.Models.IdentityResource ToModel(IdentityServer4.EntityFramework.Entities.IdentityResource p1)
        {
            return p1 == null ? null : new IdentityServer4.Models.IdentityResource()
            {
                Required = p1.Required,
                Emphasize = p1.Emphasize,
                Enabled = p1.Enabled,
                Name = p1.Name,
                DisplayName = p1.DisplayName,
                Description = p1.Description,
                ShowInDiscoveryDocument = p1.ShowInDiscoveryDocument,
                UserClaims = p1.UserClaims == null ? new System.Collections.Generic.HashSet<string>() : p1.UserClaims.Select<IdentityServer4.EntityFramework.Entities.IdentityResourceClaim, string>(funcMain1).ToHashSet<string>(),
                Properties = p1.Properties == null ? new System.Collections.Generic.Dictionary<string, string>() : p1.Properties.ToDictionary<IdentityServer4.EntityFramework.Entities.IdentityResourceProperty, string, string>(funcMain2, funcMain3)
            };
        }
        public IdentityServer4.EntityFramework.Entities.IdentityResource ToEntity(IdentityServer4.Models.IdentityResource p2)
        {
            return p2 == null ? null : new IdentityServer4.EntityFramework.Entities.IdentityResource()
            {
                Enabled = p2.Enabled,
                Name = p2.Name,
                DisplayName = p2.DisplayName,
                Description = p2.Description,
                Required = p2.Required,
                Emphasize = p2.Emphasize,
                ShowInDiscoveryDocument = p2.ShowInDiscoveryDocument,
                UserClaims = p2.UserClaims == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.IdentityResourceClaim>() : p2.UserClaims.Select<string, IdentityServer4.EntityFramework.Entities.IdentityResourceClaim>(funcMain4).ToList<IdentityServer4.EntityFramework.Entities.IdentityResourceClaim>(),
                Properties = p2.Properties == null ? new System.Collections.Generic.List<IdentityServer4.EntityFramework.Entities.IdentityResourceProperty>() : p2.Properties.Select<System.Collections.Generic.KeyValuePair<string, string>, IdentityServer4.EntityFramework.Entities.IdentityResourceProperty>(funcMain5).ToList<IdentityServer4.EntityFramework.Entities.IdentityResourceProperty>()
            };
        }
        
        private string funcMain1(IdentityServer4.EntityFramework.Entities.IdentityResourceClaim x)
        {
            return x.Type;
        }
        
        private string funcMain2(IdentityServer4.EntityFramework.Entities.IdentityResourceProperty x)
        {
            return x.Key;
        }
        
        private string funcMain3(IdentityServer4.EntityFramework.Entities.IdentityResourceProperty x)
        {
            return x.Value;
        }
        
        private IdentityServer4.EntityFramework.Entities.IdentityResourceClaim funcMain4(string x)
        {
            return new IdentityServer4.EntityFramework.Entities.IdentityResourceClaim() {Type = x};
        }
        
        private IdentityServer4.EntityFramework.Entities.IdentityResourceProperty funcMain5(System.Collections.Generic.KeyValuePair<string, string> x)
        {
            return new IdentityServer4.EntityFramework.Entities.IdentityResourceProperty()
            {
                Key = x.Key,
                Value = x.Value
            };
        }
    }
}