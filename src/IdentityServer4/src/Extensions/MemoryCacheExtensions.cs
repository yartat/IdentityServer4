// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityServer4.Extensions
{
    internal static class MemoryCacheExtensions
    {
        /// <summary>
        /// Returns the value cached under <paramref name="key"/>, or creates it with <paramref name="factory"/>
        /// and caches it for <paramref name="duration"/>. A zero or negative duration bypasses the cache.
        /// </summary>
        public static async Task<T> GetOrCreateWithExpirationAsync<T>(this IMemoryCache cache, string key, TimeSpan duration, Func<Task<T>> factory)
        {
            if (duration <= TimeSpan.Zero)
            {
                return await factory();
            }

            return await cache.GetOrCreateAsync(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = duration;
                return factory();
            });
        }
    }
}
