// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer4.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task WriteJsonAsync(this HttpResponse response, object o, string? contentType = null)
        {
            var json = ObjectSerializer.ToBuffer(o);
            response.ContentType = contentType ??  "application/json; charset=UTF-8";
            await response.Body.WriteAsync(json);
            await response.Body.FlushAsync();
        }

        public static async Task WriteJsonAsync(this HttpResponse response, string json, string? contentType = null)
        {
            response.ContentType = contentType ?? "application/json; charset=UTF-8";
            await response.WriteAsync(json);
            await response.Body.FlushAsync();
        }

        public static void SetCache(this HttpResponse response, int maxAge, params string[] varyBy)
        {
            if (maxAge == 0)
            {
                SetNoCache(response);
            }
            else if (maxAge > 0)
            {
                response.Headers[HeaderNames.CacheControl] = $"max-age={maxAge}";

                if (varyBy?.Any() == true)
                {
                    var vary = varyBy.Aggregate((x, y) => x + "," + y);
                    if (response.Headers.TryGetValue(HeaderNames.Vary, out var existingVary))
                    {
                        vary = existingVary.ToString() + "," + vary;
                    }

                    response.Headers[HeaderNames.Vary] = vary;
                }
            }
        }

        public static void SetNoCache(this HttpResponse response)
        {
            response.Headers[HeaderNames.CacheControl] = "no-store, no-cache, max-age=0";
            response.Headers[HeaderNames.Pragma] = "no-cache";
        }

        public static async Task WriteHtmlAsync(this HttpResponse response, string html)
        {
            response.ContentType = "text/html; charset=UTF-8";
            await response.WriteAsync(html, Encoding.UTF8);
            await response.Body.FlushAsync();
        }

        public static void RedirectToAbsoluteUrl(this HttpResponse response, string url)
        {
            if (url.IsLocalUrl())
            {
                if (url.StartsWith("~/")) url = url.Substring(1);
                url = response.HttpContext.GetIdentityServerBaseUri() + url.RemoveLeadingSlash();
            }
            response.Redirect(url);
        }

        public static void AddScriptCspHeaders(this HttpResponse response, CspOptions options, string hash)
        {
            var csp1part = options.Level == CspLevel.One ? "'unsafe-inline' " : string.Empty;
            var cspHeader = $"default-src 'none'; script-src {csp1part}'{hash}'";

            AddCspHeaders(response.Headers, options, cspHeader);
        }

        public static void AddStyleCspHeaders(
            this HttpResponse response,
            CspOptions options,
            string hash,
            string? frameSources)
        {
            var csp1part = options.Level == CspLevel.One ? "'unsafe-inline' " : string.Empty;
            var cspHeader = $"default-src 'none'; style-src {csp1part}'{hash}'";

            if (!string.IsNullOrEmpty(frameSources))
            {
                cspHeader += $"; frame-src {frameSources}";
            }

            AddCspHeaders(response.Headers, options, cspHeader);
        }

        public static void AddCspHeaders(this IHeaderDictionary headers, CspOptions options, string cspHeader)
        {
            if (!headers.ContainsKey(HeaderNames.ContentSecurityPolicy))
            {
                headers[HeaderNames.ContentSecurityPolicy] = cspHeader;
            }
            
            if (options.AddDeprecatedHeader && !headers.ContainsKey("X-Content-Security-Policy"))
            {
                headers["X-Content-Security-Policy"] = cspHeader;
            }
        }
    }
}