// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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

        internal static bool GetSignOutCalled(this HttpContext context) =>
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

            EndSession endSessionMsg = null;

            // if we have a logout message, then that take precedence over the current user
            if (logoutMessage?.ClientIds?.Any() == true)
            {
                var clientIds = logoutMessage?.ClientIds;

                // check if current user is same, since we migth have new clients (albeit unlikely)
                if (currentSubId == logoutMessage?.SubjectId)
                {
                    clientIds = clientIds.Union(await userSession.GetClientListAsync());
                    clientIds = clientIds.Distinct();
                }

                endSessionMsg = new EndSession
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
                    endSessionMsg = new EndSession
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
                var msg = new Message<EndSession>(endSessionMsg, clock.UtcNow.UtcDateTime);

                var endSessionMessageStore = context.RequestServices.GetRequiredService<IMessageStore<EndSession>>();
                var id = await endSessionMessageStore.WriteAsync(msg);

                var signoutIframeUrl = context.GetIdentityServerBaseUri() + Constants.ProtocolRoutePaths.EndSessionCallback;
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
        /// <param name="tryUseXForwardHeader">Use X-Forwarded-For header</param>
        /// <returns>Returns IP address of the specified HTTP context object.</returns>
        public static string GetRequestIp(this HttpContext context,
            bool tryUseXForwardHeader = true)
        {
            string ip = null;

            // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

            // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
            // for 99% of cases however it has been suggested that a better (although tedious)
            // approach might be to read each IP from right to left and use the first public IP.
            // http://stackoverflow.com/a/43554000/538763
            //
            if (tryUseXForwardHeader)
            {
                ip = context.GetHeaderValueAs<string>("X-Forwarded-For").SplitCsv().FirstOrDefault();
            }

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (string.IsNullOrWhiteSpace(ip) && context?.Connection?.RemoteIpAddress != null)
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = context.GetHeaderValueAs<string>("REMOTE_ADDR");
            }

            return ip;
        }

        public static T GetHeaderValueAs<T>(this HttpContext context, string headerName)
        {
            if (context?.Request?.Headers?.TryGetValue(headerName, out var values) ?? false)
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