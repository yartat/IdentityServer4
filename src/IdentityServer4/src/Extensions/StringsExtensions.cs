// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;

namespace IdentityServer4.Extensions
{
    /// <summary>
    /// Defines a string extension methods.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Maximum length of the device name taken from the user agent.
        /// </summary>
        public const int MaxDeviceLength = 200;

        /// <summary>
        /// Gets the device by user agent string.
        /// The user agent is set by the caller: only printable ASCII is kept and the result is at most <see cref="MaxDeviceLength"/> characters,
        /// since it ends up in tokens, sessions and logs.
        /// </summary>
        /// <param name="userAgent">The user agent string.</param>
        /// <returns>Returns device name.</returns>
        public static string GetDevice(this string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return null;
            }

            var device = new StringBuilder(Math.Min(userAgent.Length, MaxDeviceLength));
            foreach (var c in userAgent)
            {
                if (device.Length == MaxDeviceLength)
                {
                    break;
                }

                if (c >= ' ' && c <= '~')
                {
                    device.Append(c);
                }
            }

            var result = device.ToString().Trim();
            return result.Length == 0 ? null : result;
        }

        /// <summary>
        /// Converts to space separated string.
        /// </summary>
        /// <param name="list">The list of strings.</param>
        /// <returns>Returns the space separated string.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToSpaceSeparatedString(this IEnumerable<string> list) =>
            list == null ?
                string.Empty :
                string.Join(' ', list).Trim();

        /// <summary>
        /// Converts from the space separated string.
        /// </summary>
        /// <param name="input">The space separated string.</param>
        /// <returns>Returns the list of strings.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> FromSpaceSeparatedString(this string input) =>
            input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Parses the scopes string.
        /// </summary>
        /// <param name="scopes">The scopes.</param>
        /// <returns>Returns the list of scopes.</returns>
        public static List<string> ParseScopesString(this string scopes)
        {
            if (scopes.IsMissing())
            {
                return null;
            }

            scopes = scopes.Trim();
            var parsedScopes = scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

            if (parsedScopes.Count > 0)
            {
                parsedScopes.Sort();
                return parsedScopes;
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified value is missing.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the specified value is missing; otherwise, <c>false</c>.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMissing(this string value) =>
            string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Determines whether is missing or too long the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns><c>true</c> if is missing or too long the specified value; otherwise, <c>false</c>.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMissingOrTooLong(this string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }
            if (value.Length > maxLength)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified value is present.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the specified value is present; otherwise, <c>false</c>.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPresent(this string value) =>
            !string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Ensures the leading slash in path.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Returns the URL with leading slash in path.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsureLeadingSlash(this string url) =>
            url?.StartsWith("/") == false ?
                "/" + url :
                url;

        /// <summary>
        /// Ensures the trailing slash.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Returns the URL with trailing slash in path.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsureTrailingSlash(this string url) =>
            url?.EndsWith("/") == false ?
                url + "/" :
                url;

        /// <summary>
        /// Removes the leading slash.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Returns the URL without leading slash in path.</returns>
        [DebuggerStepThrough]
        public static string RemoveLeadingSlash(this string url)
        {
            if (url?.StartsWith("/") == true)
            {
                url = url.Substring(1);
            }

            return url;
        }

        /// <summary>
        /// Removes the trailing slash.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Returns the URL without trailing slash in path.</returns>
        [DebuggerStepThrough]
        public static string RemoveTrailingSlash(this string url)
        {
            if (url?.EndsWith("/") == true)
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        /// <summary>
        /// Cleans the URL path.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>System.String.</returns>
        [DebuggerStepThrough]
        public static string CleanUrlPath(this string url)
        {
            if (string.IsNullOrWhiteSpace(url)) url = "/";

            if (url != "/" && url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        /// <summary>
        /// Determines whether is local URL the specified value.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns><c>true</c> if is local URL the specified value; otherwise, <c>false</c>.</returns>
        [DebuggerStepThrough]
        public static bool IsLocalUrl(this string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            // Allows "/" or "/foo" but not "//" or "/\".
            if (url[0] == '/')
            {
                // url is exactly "/"
                if (url.Length == 1)
                {
                    return true;
                }

                // url doesn't start with "//" or "/\"
                if (url[1] != '/' && url[1] != '\\')
                {
                    return true;
                }

                return false;
            }

            // Allows "~/" or "~/foo" but not "~//" or "~/\".
            if (url[0] == '~' && url.Length > 1 && url[1] == '/')
            {
                // url is exactly "~/"
                if (url.Length == 2)
                {
                    return true;
                }

                // url doesn't start with "~//" or "~/\"
                if (url[2] != '/' && url[2] != '\\')
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Adds the query string to URL.
        /// </summary>
        /// <param name="url">The source URL.</param>
        /// <param name="query">The query to add.</param>
        /// <returns>Returns URL with query.</returns>
        [DebuggerStepThrough]
        public static string AddQueryString(this string url, string query)
        {
            if (!url.Contains("?"))
            {
                url += "?";
            }
            else if (!url.EndsWith("&"))
            {
                url += "&";
            }

            return url + query;
        }

        /// <summary>
        /// Adds the query parameters with value to URL.
        /// </summary>
        /// <param name="url">The source URL.</param>
        /// <param name="name">The query parameter name.</param>
        /// <param name="value">The query parameter value.</param>
        /// <returns>Returns URL with query.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AddQueryString(this string url, string name, string value) =>
            url.AddQueryString(name + "=" + UrlEncoder.Default.Encode(value));

        /// <summary>
        /// Adds the hash fragment.
        /// </summary>
        /// <param name="url">The source URL.</param>
        /// <param name="query">The query to add with hash.</param>
        /// <returns>Returns URL with hash fragment.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AddHashFragment(this string url, string query)
        {
            if (!url.Contains("#"))
            {
                url += "#";
            }

            return url + query;
        }

        /// <summary>
        /// Reads the query string as name value collection.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Returns name value collection from query.</returns>
        [DebuggerStepThrough]
        public static NameValueCollection ReadQueryStringAsNameValueCollection(this string url)
        {
            if (url != null)
            {
                var idx = url.IndexOf('?');
                if (idx >= 0)
                {
                    url = url.Substring(idx + 1);
                }
                var query = QueryHelpers.ParseNullableQuery(url);
                if (query != null)
                {
                    return query.AsNameValueCollection();
                }
            }

            return new NameValueCollection();
        }

        /// <summary>
        /// Gets the origin.
        /// </summary>
        /// <param name="url">The source URL.</param>
        /// <returns>Returns origin.</returns>
        public static string GetOrigin(this string url)
        {
            if (url != null)
            {
                Uri uri;
                try
                {
                    uri = new Uri(url);
                }
                catch (Exception)
                {
                    return null;
                }

                if (uri.Scheme == "http" || uri.Scheme == "https")
                {
                    return $"{uri.Scheme}://{uri.Authority}";
                }
            }

            return null;
        }

        /// <summary>
        /// Obfuscates the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public static string Obfuscate(this string value)
        {
            var last4Chars = "****";
            if (value.IsPresent() && value.Length > 4)
            {
                last4Chars = value.Substring(value.Length - 4);
            }

            return "****" + last4Chars;
        }
    }
}