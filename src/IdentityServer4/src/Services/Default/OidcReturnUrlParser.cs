// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using IdentityServer4.Stores;
using System.Collections.Specialized;

namespace IdentityServer4.Services
{
    internal class OidcReturnUrlParser : IReturnUrlParser
    {
        private readonly IAuthorizeRequestValidator _validator;
        private readonly IUserSession _userSession;
        private readonly ILogger _logger;
        private readonly IAuthorizationParametersMessageStore _authorizationParametersMessageStore;
        private readonly HashSet<string> _allowedOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public OidcReturnUrlParser(
            IAuthorizeRequestValidator validator,
            IUserSession userSession,
            IdentityServerOptions options,
            ILogger<OidcReturnUrlParser> logger,
            IAuthorizationParametersMessageStore authorizationParametersMessageStore = null)
        {
            _validator = validator;
            _userSession = userSession;
            _logger = logger;
            _authorizationParametersMessageStore = authorizationParametersMessageStore;

            // LoginPageResult makes the return URL absolute on BaseUri when the login page is hosted elsewhere
            if (options.BaseUri.IsPresent())
            {
                AddAllowedOrigin(options.BaseUri, nameof(IdentityServerOptions.BaseUri));
            }

            foreach (var origin in options.UserInteraction.AllowedReturnUrlOrigins ?? Array.Empty<string>())
            {
                AddAllowedOrigin(origin, nameof(UserInteractionOptions.AllowedReturnUrlOrigins));
            }
        }

        public async Task<AuthorizationRequest> ParseAsync(string returnUrl)
        {
            if (IsValidReturnUrl(returnUrl))
            {
                var parameters = returnUrl.ReadQueryStringAsNameValueCollection();
                if (_authorizationParametersMessageStore != null)
                {
                    var messageStoreId = parameters[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
                    var entry = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);
                    parameters = entry?.Data.FromFullDictionary() ?? new NameValueCollection();
                }

                var user = await _userSession.GetUserAsync();
                var result = await _validator.ValidateAsync(parameters, user);
                if (!result.IsError)
                {
                    _logger.LogTrace("AuthorizationRequest being returned");
                    return new AuthorizationRequest(result.ValidatedRequest);
                }
            }

            _logger.LogTrace("No AuthorizationRequest being returned");
            return null;
        }

        public bool IsValidReturnUrl(string returnUrl)
        {
            string path;
            if (returnUrl.IsLocalUrl())
            {
                path = returnUrl;
                var index = path.IndexOf('?');
                if (index >= 0)
                {
                    path = path.Substring(0, index);
                }
            }
            else if (TryGetAllowedAbsoluteUri(returnUrl, out var uri))
            {
                path = uri.AbsolutePath;
            }
            else
            {
                _logger.LogTrace("returnUrl is neither local nor on an allowed origin");
                return false;
            }

            if (path.EndsWith(Constants.ProtocolRoutePaths.Authorize, StringComparison.Ordinal) ||
                path.EndsWith(Constants.ProtocolRoutePaths.AuthorizeCallback, StringComparison.Ordinal))
            {
                _logger.LogTrace("returnUrl is valid");
                return true;
            }

            _logger.LogTrace("returnUrl is not valid");
            return false;
        }

        private bool TryGetAllowedAbsoluteUri(string url, out Uri uri) =>
            TryGetHttpUri(url, out uri) && _allowedOrigins.Contains(uri.GetLeftPart(UriPartial.Authority));

        private void AddAllowedOrigin(string url, string source)
        {
            if (TryGetHttpUri(url, out var uri))
            {
                _allowedOrigins.Add(uri.GetLeftPart(UriPartial.Authority));
            }
            else
            {
                _logger.LogWarning("Ignoring {source} entry {origin}: an absolute http(s) URL is expected", source, url);
            }
        }

        private static bool TryGetHttpUri(string url, out Uri uri) =>
            Uri.TryCreate(url, UriKind.Absolute, out uri) &&
            (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}
