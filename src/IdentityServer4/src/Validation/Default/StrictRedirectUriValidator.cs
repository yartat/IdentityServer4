// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Default implementation of redirect URI validator. Validates the URIs against
    /// the client's configured URIs.
    /// </summary>
    /// <remarks>
    /// Absolute URIs are compared as strings (ordinal ignore case), as required by OpenID Connect ("simple string comparison").
    /// Requested URIs with a fragment or that are not absolute URIs are never valid. A registered relative URI (a path) only
    /// matches on the origins in <see cref="ValidationOptions.AllowedRelativeRedirectUriOrigins"/>.
    /// </remarks>
    /// <seealso cref="IdentityServer4.Validation.IRedirectUriValidator" />
    public class StrictRedirectUriValidator : IRedirectUriValidator
    {
        private readonly HashSet<string> _relativeRedirectUriOrigins;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrictRedirectUriValidator"/> class that accepts only absolute registered URIs.
        /// </summary>
        public StrictRedirectUriValidator()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StrictRedirectUriValidator"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public StrictRedirectUriValidator(IdentityServerOptions options)
        {
            _relativeRedirectUriOrigins = new HashSet<string>(
                (options?.Validation.AllowedRelativeRedirectUriOrigins ?? Enumerable.Empty<string>())
                    .Select(GetOrigin)
                    .Where(origin => origin != null),
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a given URI string is in a collection of absolute URIs (using ordinal ignore case comparison).
        /// </summary>
        /// <param name="uris">The uris.</param>
        /// <param name="requestedUri">The requested URI.</param>
        /// <returns></returns>
        protected static bool StringCollectionContainsString(IEnumerable<Uri> uris, string requestedUri) =>
            IsWellFormed(requestedUri) &&
            uris?.Any(uri => uri != null && uri.IsAbsoluteUri && string.Equals(uri.OriginalString, requestedUri, StringComparison.OrdinalIgnoreCase)) == true;

        /// <summary>
        /// Checks if the requested URI is one of the registered URIs, including registered relative URIs on an allowed origin.
        /// </summary>
        /// <param name="uris">The registered URIs.</param>
        /// <param name="requestedUri">The requested URI.</param>
        /// <returns></returns>
        protected bool IsRegistered(IEnumerable<Uri> uris, string requestedUri)
        {
            if (StringCollectionContainsString(uris, requestedUri))
            {
                return true;
            }

            if (_relativeRedirectUriOrigins.Count == 0 || !IsWellFormed(requestedUri) ||
                !_relativeRedirectUriOrigins.Contains(GetOrigin(requestedUri) ?? string.Empty))
            {
                return false;
            }

            var pathAndQuery = new Uri(requestedUri, UriKind.Absolute).PathAndQuery;
            return uris?.Any(uri => uri != null && !uri.IsAbsoluteUri && string.Equals(uri.OriginalString, pathAndQuery, StringComparison.OrdinalIgnoreCase)) == true;
        }

        /// <summary>
        /// Determines whether a redirect URI is valid for a client.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        ///   <c>true</c> is the URI is valid; <c>false</c> otherwise.
        /// </returns>
        public virtual Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client) =>
            Task.FromResult(IsRegistered(client.RedirectUris, requestedUri));

        /// <summary>
        /// Determines whether a post logout URI is valid for a client.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        ///   <c>true</c> is the URI is valid; <c>false</c> otherwise.
        /// </returns>
        public virtual Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client) =>
            Task.FromResult(IsRegistered(client.PostLogoutRedirectUris, requestedUri));

        // a redirect URI must be absolute and must not have a fragment (RFC 6749 3.1.2)
        private static bool IsWellFormed(string requestedUri) =>
            requestedUri.IsPresent() && !requestedUri.Contains('#') && Uri.TryCreate(requestedUri, UriKind.Absolute, out _);

        private static string GetOrigin(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                ? uri.GetLeftPart(UriPartial.Authority)
                : null;
    }
}
