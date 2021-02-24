// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Default implementation of redirect URI validator. Validates the URIs against
    /// the client's configured URIs.
    /// </summary>
    public class StrictRedirectUriValidator : IRedirectUriValidator
    {
        private static readonly UriComparer Comparer = new UriComparer();

        /// <summary>
        /// Checks if a given URI string is in a collection of strings (using ordinal ignore case comparison)
        /// </summary>
        /// <param name="uris">The uris.</param>
        /// <param name="requestedUri">The requested URI.</param>
        /// <returns></returns>
        protected bool StringCollectionContainsString(IEnumerable<Uri> uris, string requestedUri)
        {
            if (uris.IsNullOrEmpty()) return false;

            return uris.Contains(new Uri(requestedUri), Comparer);
        }

        /// <summary>
        /// Determines whether a redirect URI is valid for a client.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        ///   <c>true</c> is the URI is valid; <c>false</c> otherwise.
        /// </returns>
        public virtual Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(StringCollectionContainsString(client.RedirectUris, requestedUri));
        }

        /// <summary>
        /// Determines whether a post logout URI is valid for a client.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        ///   <c>true</c> is the URI is valid; <c>false</c> otherwise.
        /// </returns>
        public virtual Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(StringCollectionContainsString(client.PostLogoutRedirectUris, requestedUri));
        }

        private sealed class UriComparer : IEqualityComparer<Uri>
        {
            public bool Equals([AllowNull] Uri x, [AllowNull] Uri y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (x.IsAbsoluteUri != y.IsAbsoluteUri)
                {
                    var pathX = x.IsAbsoluteUri ? x.PathAndQuery : x.OriginalString;
                    var pathY = y.IsAbsoluteUri ? y.PathAndQuery : y.OriginalString;
                    return string.Equals(pathX, pathY, StringComparison.OrdinalIgnoreCase);
                }

                return x.Equals(y);
            }

            public int GetHashCode([DisallowNull] Uri obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
