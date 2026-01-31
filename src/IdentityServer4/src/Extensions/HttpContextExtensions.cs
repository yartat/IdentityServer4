// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

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
        /// <summary>
        /// Determines whether the specified authentication scheme supports sign out.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="scheme">The authentication scheme name.</param>
        /// <returns>A task that returns true if the scheme's handler implements IAuthenticationSignOutHandler; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when context or scheme is null or empty.</exception>
        public static async ValueTask<bool> GetSchemeSupportsSignOutAsync(
            this HttpContext? context,
            string? scheme)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(scheme))
            {
                throw new ArgumentNullException(nameof(scheme));
            }

            var provider = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            var handler = await provider.GetHandlerAsync(context, scheme);
            return handler is not null and IAuthenticationSignOutHandler;
        }

        /// <summary>
        /// Sets the IdentityServer origin by parsing and applying the scheme and host to the HTTP request.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="value">The origin value in the format "scheme://host" (e.g., "https://example.com").</param>
        /// <remarks>
        /// This method parses the provided origin value into scheme and host components separated by "://",
        /// then updates the request's Scheme and Host properties accordingly.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when context or value is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when value does not contain exactly one "://" separator.</exception>
        public static void SetIdentityServerOrigin(this HttpContext? context, string? value)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            var split = value.Split("://", StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
            {
                throw new ArgumentException("Invalid origin value", nameof(value));
            }

            var request = context.Request;
            request.Scheme = split[0];
            request.Host = new HostString(split[1]);
        }

        public static void SetIdentityServerBasePath(this HttpContext? context, string value)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Items[Constants.EnvironmentKeys.IdentityServerBasePath] = value;
        }

        public static string GetIdentityServerOrigin(this HttpContext? context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var request = context.Request;

            if (options.MutualTls.Enabled && options.MutualTls.DomainName.IsPresent())
            {
                if (!options.MutualTls.DomainName.Contains("."))
                {
                    if (request.Host.Value.StartsWith(options.MutualTls.DomainName, StringComparison.OrdinalIgnoreCase))
                    {
                        return request.Scheme + "://" +
                               request.Host.Value[(options.MutualTls.DomainName.Length + 1)..];
                    }
                }
            }

            return request.Scheme + "://" + request.Host.Value;
        }


        internal static void SetSignOutCalled(this HttpContext? context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Items[Constants.EnvironmentKeys.SignOutCalled] = "true";
        }

        public static bool GetSignOutCalled(this HttpContext? context) =>
            context?.Items.ContainsKey(Constants.EnvironmentKeys.SignOutCalled) ?? false;

        /// <summary>
        /// Gets the host name of IdentityServer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static string GetIdentityServerHost(this HttpContext? context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var request = context.Request;
            return request.Scheme + "://" + request.Host.ToUriComponent();
        }

        /// <summary>
        /// Gets the base path of IdentityServer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static string GetIdentityServerBasePath(this HttpContext? context) =>
            context?.Items[Constants.EnvironmentKeys.IdentityServerBasePath] as string ?? string.Empty;

        /// <summary>
        /// Gets the identity server relative URL by combining the base URI with the provided path.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="path">The relative path (can include "~/" prefix which will be removed).</param>
        /// <returns>The complete identity server relative URL if the path is a local URL; otherwise an empty string.</returns>
        /// <remarks>
        /// This method validates that the provided path is a local URL before constructing the full URL.
        /// If the path starts with "~/", it will be stripped before appending to the base URI.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
        public static string GetIdentityServerRelativeUrl(
            this HttpContext? context,
            string? path)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var pathSpan = path.AsSpan();
            if (!pathSpan.IsLocalUrl())
            {
                return string.Empty;
            }

            if (pathSpan.StartsWith("~/")) pathSpan = pathSpan[1..];
            return string.Concat(context.GetIdentityServerBaseUri(), pathSpan.RemoveLeadingSlash());
        }

        /// <summary>
        /// Gets the identity server relative URL by combining the base URI with the provided path.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="path">The relative path (can include "~/" prefix which will be removed).</param>
        /// <returns>The complete identity server relative URL if the path is a local URL; otherwise an empty string.</returns>
        /// <remarks>
        /// This method validates that the provided path is a local URL before constructing the full URL.
        /// If the path starts with "~/", it will be stripped before appending to the base URI.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
        public static string GetIdentityServerRelativeUrl(
            this HttpContext? context,
            ReadOnlySpan<char> path)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (path.IsEmpty)
            {
                return string.Empty;
            }

            if (!path.IsLocalUrl())
            {
                return string.Empty;
            }

            if (path.StartsWith("~/")) path = path[1..];
            return string.Concat(context.GetIdentityServerBaseUri(), path.RemoveLeadingSlash());
        }

        /// <summary>
        /// Gets the identity server issuer URI.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns issuer URI.</returns>
        /// <exception cref="System.ArgumentNullException">context</exception>
        public static string GetIdentityServerIssuerUri(this HttpContext? context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // if they've explicitly configured a URI then use it,
            // otherwise dynamically calculate it
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var uri = options.IssuerUri.AsSpan();
            if (uri.IsEmpty)
            {
                ReadOnlySpan<char> origin = context.GetIdentityServerOrigin();
                ReadOnlySpan<char> basePath = context.GetIdentityServerBasePath();
                uri = string.Concat(origin, basePath).AsSpan();
                if (uri.EndsWith("/"))
                {
                    uri = uri[..^1];
                }

                if (options.LowerCaseIssuerUri)
                {
                    Span<char> result = stackalloc char[uri.Length];
                    uri.ToLowerInvariant(result);
                    return result.ToString();
                }
           }

            return uri.ToString();
        }

        /// <summary>
        /// Gets the identity server base URI.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns base URI</returns>
        /// <exception cref="System.ArgumentNullException">context</exception>
        public static string GetIdentityServerBaseUri(this HttpContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // if they've explicitly configured a URI then use it,
            // otherwise dynamically calculate it
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var uri = options.BaseUri.IsMissing() ?
                string.Concat(context.GetIdentityServerHost(), context.GetIdentityServerBasePath()) :
                options.BaseUri;

            return options.LowerCaseIssuerUri ?
                uri.ToLower().EnsureTrailingSlash() :
                uri.EnsureTrailingSlash();
        }

        internal static async Task<string?> GetIdentityServerSignoutFrameCallbackUrlAsync(
            this HttpContext? context,
            LogoutMessage? logoutMessage = null)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var userSession = context.RequestServices.GetRequiredService<IUserSession>();
            var user = await userSession.GetUserAsync();
            var currentSubId = user?.GetSubjectId();

            LogoutNotificationContext? endSessionMsg = null;

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
        /// <param name="tryUseXForwardHeader">Use X-Forwarded-For header</param>
        /// <returns>Returns IP address of the specified HTTP context object.</returns>
        public static string GetRequestIp(
            this HttpContext? context,
            bool tryUseXForwardHeader = true)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            string? ip = null;

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