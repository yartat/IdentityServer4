// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

#pragma warning disable 1591

namespace IdentityServer4.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task<bool> GetSchemeSupportsSignOutAsync(this HttpContext context, string scheme)
        {
            var provider = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            var handler = await provider.GetHandlerAsync(context, scheme);
            return (handler != null && handler is IAuthenticationSignOutHandler);
        }

        public static void SetIdentityServerOrigin(this HttpContext context, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var split = value.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries);

            var request = context.Request;
            request.Scheme = split.First();
            request.Host = new HostString(split.Last());
        }

        public static void SetIdentityServerBasePath(this HttpContext context, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.Items[Constants.EnvironmentKeys.IdentityServerBasePath] = value;
        }

        public static string GetIdentityServerOrigin(this HttpContext context)
        {
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var request = context.Request;

            if (options.MutualTls.Enabled && options.MutualTls.DomainName.IsPresent())
            {
                if (!options.MutualTls.DomainName.Contains("."))
                {
                    if (request.Host.Value.StartsWith(options.MutualTls.DomainName, StringComparison.OrdinalIgnoreCase))
                    {
                        return request.Scheme + "://" +
                               request.Host.Value.Substring(options.MutualTls.DomainName.Length + 1);
                    }
                }
            }

            return request.Scheme + "://" + request.Host.Value;
        }


        internal static void SetSignOutCalled(this HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.Items[Constants.EnvironmentKeys.SignOutCalled] = "true";
        }

        public static bool GetSignOutCalled(this HttpContext context) =>
            context.Items.ContainsKey(Constants.EnvironmentKeys.SignOutCalled);

        /// <summary>
        /// Gets the host name of IdentityServer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static string GetIdentityServerHost(this HttpContext context)
        {
            var request = context.Request;
            return request.Scheme + "://" + request.Host.ToUriComponent();
        }

        /// <summary>
        /// Gets the base path of IdentityServer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static string GetIdentityServerBasePath(this HttpContext context) =>
            context.Items[Constants.EnvironmentKeys.IdentityServerBasePath] as string;

        /// <summary>
        /// Gets the identity server relative URL.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string GetIdentityServerRelativeUrl(this HttpContext context, string path)
        {
            if (!path.IsLocalUrl())
            {
                return null;
            }

            if (path.StartsWith("~/")) path = path.Substring(1);
            return context.GetIdentityServerBaseUri() + path.RemoveLeadingSlash();
        }

        /// <summary>
        /// Gets the identity server issuer URI.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns issuer URI.</returns>
        /// <exception cref="System.ArgumentNullException">context</exception>
        public static string GetIdentityServerIssuerUri(this HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // if they've explicitly configured a URI then use it,
            // otherwise dynamically calculate it
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var uri = options.IssuerUri;
            if (uri.IsMissing())
            {
                uri = context.GetIdentityServerOrigin() + context.GetIdentityServerBasePath();
                if (uri.EndsWith("/")) uri = uri.Substring(0, uri.Length - 1);
                if (options.LowerCaseIssuerUri)
                {
                    uri = uri.ToLowerInvariant();
                }
            }

            return uri;
        }

        /// <summary>
        /// Gets the identity server base URI.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns base URI</returns>
        /// <exception cref="System.ArgumentNullException">context</exception>
        public static string GetIdentityServerBaseUri(this HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // if they've explicitly configured a URI then use it,
            // otherwise dynamically calculate it
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var uri = options.BaseUri;
            if (uri.IsMissing())
            {
                uri = context.GetIdentityServerHost() + context.GetIdentityServerBasePath();
            }

            if (options.LowerCaseIssuerUri)
            {
                uri = uri?.ToLower();
            }

            return uri?.EnsureTrailingSlash();
        }

        internal static async Task<string> GetIdentityServerSignoutFrameCallbackUrlAsync(this HttpContext context, LogoutMessage logoutMessage = null)
        {
            var userSession = context.RequestServices.GetRequiredService<IUserSession>();
            var user = await userSession.GetUserAsync();
            var currentSubId = user?.GetSubjectId();

            LogoutNotificationContext endSessionMsg = null;

            // if we have a logout message, then that take precedence over the current user
            if (logoutMessage?.ClientIds?.Any() == true)
            {
                var clientIds = logoutMessage.ClientIds;

                // check if current user is same, since we might have new clients (albeit unlikely)
                if (currentSubId == logoutMessage.SubjectId)
                {
                    clientIds = clientIds.Union(await userSession.GetClientListAsync());
                    clientIds = clientIds.Distinct();
                }

                endSessionMsg = new LogoutNotificationContext
                {
                    SubjectId = logoutMessage.SubjectId,
                    SessionId = logoutMessage.SessionId,
                    ClientIds = clientIds
                };
            }
            else if (currentSubId != null)
            {
                // see if current user has any clients they need to signout of 
                var clientIds = await userSession.GetClientListAsync();
                if (clientIds.Any())
                {
                    endSessionMsg = new LogoutNotificationContext
                    {
                        SubjectId = currentSubId,
                        SessionId = await userSession.GetSessionIdAsync(),
                        ClientIds = clientIds
                    };
                }
            }

            if (endSessionMsg != null)
            {
                var clock = context.RequestServices.GetRequiredService<ISystemClock>();
                var msg = new Message<LogoutNotificationContext>(endSessionMsg, clock.UtcNow.UtcDateTime);

                var endSessionMessageStore = context.RequestServices.GetRequiredService<IMessageStore<LogoutNotificationContext>>();
                var id = await endSessionMessageStore.WriteAsync(msg);

                var signoutIframeUrl = context.GetIdentityServerBaseUri().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.EndSessionCallback;
                signoutIframeUrl = signoutIframeUrl.AddQueryString(Constants.UIConstants.DefaultRoutePathParams.EndSessionCallback, id);

                return signoutIframeUrl;
            }

            // no sessions, so nothing to cleanup
            return null;
        }

        /// <summary>
        /// Extracts client IP address from context
        /// </summary>
        /// <param name="context">HTTP context object.</param>
        /// <param name="tryUseXForwardHeader">
        /// Use the first entry of the X-Forwarded-For header. The header is set by the caller and can not be trusted;
        /// behind a reverse proxy use ForwardedHeadersMiddleware with KnownProxies/KnownNetworks instead, which sets
        /// the connection's remote IP address from the proxy's header.
        /// </param>
        /// <returns>Returns IP address of the specified HTTP context object.</returns>
        public static string GetRequestIp(this HttpContext context,
            bool tryUseXForwardHeader = false)
        {
            if (tryUseXForwardHeader)
            {
                var forwarded = context.GetHeaderValueAs<string>("X-Forwarded-For").SplitCsv().FirstOrDefault();
                if (IPAddress.TryParse(forwarded, out var forwardedIp))
                {
                    return forwardedIp.ToString();
                }
            }

            return context?.Connection?.RemoteIpAddress?.ToString();
        }

        public static T GetHeaderValueAs<T>(this HttpContext context, string headerName)
        {
            if (context?.Request?.Headers != null && context.Request.Headers.TryGetValue(headerName, out var values))
            {
                var rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrEmpty(rawValues))
                {
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
                }
            }

            return default(T);
        }

        private static IEnumerable<string> SplitCsv(this string csvList)
        {
            return string.IsNullOrWhiteSpace(csvList) ?
                Enumerable.Empty<string>() :
                csvList
                    .TrimEnd(',')
                    .Split(',')
                    .Select(s => s.Trim());
        }
    }
}