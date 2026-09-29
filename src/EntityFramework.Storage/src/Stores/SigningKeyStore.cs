// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.EntityFramework.Stores
{
    /// <summary>
    /// Implementation of ISigningKeyStore thats uses EF.
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.ISigningKeyStore" />
    public class SigningKeyStore : ISigningKeyStore
    {
        /// <summary>
        /// The DbContext.
        /// </summary>
        protected readonly IPersistedGrantDbContext Context;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SigningKeyStore"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public SigningKeyStore(IPersistedGrantDbContext context, ILogger<SigningKeyStore> logger)
        {
            Context = context;
            Logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<SerializedKey>> LoadKeysAsync()
        {
            var entities = await Context.Keys.AsNoTracking().ToArrayAsync();

            return entities.Select(entity => new SerializedKey
            {
                Id = entity.Id,
                Version = entity.Version,
                Created = entity.Created,
                Algorithm = entity.Algorithm,
                DataProtected = entity.DataProtected,
                Data = entity.Data
            }).ToArray();
        }

        /// <inheritdoc/>
        public virtual async Task StoreKeyAsync(SerializedKey key)
        {
            Context.Keys.Add(new Key
            {
                Id = key.Id,
                Version = key.Version,
                Created = key.Created,
                Algorithm = key.Algorithm,
                DataProtected = key.DataProtected,
                Data = key.Data
            });

            await Context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public virtual async Task DeleteKeyAsync(string id)
        {
            var entity = await Context.Keys.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return;
            }

            Context.Keys.Remove(entity);

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // another instance deleted the key first
                Logger.LogDebug("Key {kid} was already deleted: {error}", id, ex.Message);
            }
        }
    }
}
