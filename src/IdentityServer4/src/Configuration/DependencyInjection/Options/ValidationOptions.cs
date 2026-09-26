// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// The ValidationOptions contains settings that affect some of the default validation behavior.
    /// </summary>
    public class ValidationOptions
    {
        /// <summary>
        ///  Collection of URI scheme prefixes that should never be used as custom URI schemes in the redirect_uri passed to tha authorize endpoint.
        /// </summary>
        public ICollection<string> InvalidRedirectUriPrefixes { get; } = new HashSet<string>
        {
            "javascript:",
            "file:",
            "data:",
            "mailto:",
            "ftp:",
            "blob:",
            "about:",
            "ssh:",
            "tel:",
            "view-source:",
            "ws:",
            "wss:"
        };

        /// <summary>
        /// Origins (scheme, host and optional port, e.g. <c>https://app.example.com</c>) on which registered relative redirect URIs
        /// (paths such as <c>/signin-oidc</c>) are accepted. Empty by default: a relative redirect URI never matches, since it
        /// would otherwise accept the path on any host.
        /// </summary>
        public ICollection<string> AllowedRelativeRedirectUriOrigins { get; set; } = new HashSet<string>();
    }
}