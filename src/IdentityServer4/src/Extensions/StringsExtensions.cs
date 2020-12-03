// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;

namespace IdentityServer4.Extensions
{
    /// <summary>
    /// Defines a string extension methods.
    /// </summary>
    public static class StringExtensions
    {
        private static readonly DictionaryCache _cache = new DictionaryCache();

        /// <summary>
        /// Gets the device by user agent string.
        /// </summary>
        /// <param name="userAgent">The user agent string.</param>
        /// <returns>Returns device name.</returns>
        public static DeviceDetector GetDevice(this string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return null;
            }

            var result = new DeviceDetector(userAgent);
            result.SetCache(_cache);
            result.Parse();
            return result;
        }

        /// <summary>
        /// Converts to space separated string.
        /// </summary>
        /// <param name="list">The list of strings.</param>
        /// <returns>Returns the space separated string.</returns>
        [DebuggerStepThrough]
        public static string ToSpaceSeparatedString(this IEnumerable<string> list)
        {
            if (list == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(100);

            foreach (var element in list)
            {
                sb.Append(element + " ");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Converts from the space separated string.
        /// </summary>
        /// <param name="input">The space separated string.</param>
        /// <returns>Returns the list of strings.</returns>
        [DebuggerStepThrough]
        public static IEnumerable<string> FromSpaceSeparatedString(this string input)
        {
            input = input.Trim();
            return input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

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
            var parsedScopes = scopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

            if (parsedScopes.Any())
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
        public static bool IsMissing(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Determines whether is missing or too long the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns><c>true</c> if is missing or too long the specified value; otherwise, <c>false</c>.</returns>
        [DebuggerStepThrough]
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
        public static bool IsPresent(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Ensures the leading slash in path.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Returns the URL with leading slash in path.</returns>
        [DebuggerStepThrough]
        public static string EnsureLeadingSlash(this string url)
        {
            if (url != null && !url.StartsWith("/"))
            {
                return "/" + url;
            }

            return url;
        }

        /// <summary>
        /// Ensures the trailing slash.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Returns the URL with trailing slash in path.</returns>
        [DebuggerStepThrough]
        public static string EnsureTrailingSlash(this string url)
        {
            if (url != null && !url.EndsWith("/"))
            {
                return url + "/";
            }

            return url;
        }

        /// <summary>
        /// Removes the leading slash.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Returns the URL without leading slash in path.</returns>
        [DebuggerStepThrough]
        public static string RemoveLeadingSlash(this string url)
        {
            if (url != null && url.StartsWith("/"))
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
            if (url != null && url.EndsWith("/"))
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
        public static string AddQueryString(this string url, string name, string value)
        {
            return url.AddQueryString(name + "=" + UrlEncoder.Default.Encode(value));
        }

        /// <summary>
        /// Adds the hash fragment.
        /// </summary>
        /// <param name="url">The source URL.</param>
        /// <param name="query">The query to add with hash.</param>
        /// <returns>Returns URL with hash fragment.</returns>
        [DebuggerStepThrough]
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
    }
}