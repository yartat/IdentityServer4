namespace IdentityServer4.EntityFramework.Mappers
{
    public partial class PersistedGrantMapper : IdentityServer4.EntityFramework.Mappers.IPersistedGrantMapper
    {
        public IdentityServer4.Models.PersistedGrant ToModel(IdentityServer4.EntityFramework.Entities.PersistedGrant p1)
        {
            return p1 == null ? null : new IdentityServer4.Models.PersistedGrant()
            {
                Key = p1.Key,
                Type = p1.Type,
                SubjectId = p1.SubjectId,
                SessionId = p1.SessionId,
                ClientId = p1.ClientId,
                Description = p1.Description,
                CreationTime = p1.CreationTime,
                Expiration = p1.Expiration == null ? null : (System.DateTime?)(System.DateTime)p1.Expiration,
                ConsumedTime = p1.ConsumedTime == null ? null : (System.DateTime?)(System.DateTime)p1.ConsumedTime,
                Data = p1.Data
            };
        }
        public IdentityServer4.EntityFramework.Entities.PersistedGrant ToEntity(IdentityServer4.Models.PersistedGrant p2)
        {
            return p2 == null ? null : new IdentityServer4.EntityFramework.Entities.PersistedGrant()
            {
                Key = p2.Key,
                Type = p2.Type,
                SubjectId = p2.SubjectId,
                SessionId = p2.SessionId,
                ClientId = p2.ClientId,
                Description = p2.Description,
                CreationTime = p2.CreationTime,
                Expiration = p2.Expiration == null ? null : (System.DateTime?)(System.DateTime)p2.Expiration,
                ConsumedTime = p2.ConsumedTime == null ? null : (System.DateTime?)(System.DateTime)p2.ConsumedTime,
                Data = p2.Data
            };
        }
        public IdentityServer4.EntityFramework.Entities.PersistedGrant UpdateEntity(IdentityServer4.Models.PersistedGrant p3, IdentityServer4.EntityFramework.Entities.PersistedGrant p4)
        {
            if (p3 == null)
            {
                return null;
            }
            IdentityServer4.EntityFramework.Entities.PersistedGrant result = p4 ?? new IdentityServer4.EntityFramework.Entities.PersistedGrant();
            
            result.Key = p3.Key;
            result.Type = p3.Type;
            result.SubjectId = p3.SubjectId;
            result.SessionId = p3.SessionId;
            result.ClientId = p3.ClientId;
            result.Description = p3.Description;
            result.CreationTime = p3.CreationTime;
            result.Expiration = p3.Expiration == null ? null : (System.DateTime?)(System.DateTime)p3.Expiration;
            result.ConsumedTime = p3.ConsumedTime == null ? null : (System.DateTime?)(System.DateTime)p3.ConsumedTime;
            result.Data = p3.Data;
            return result;
            
        }
    }
}