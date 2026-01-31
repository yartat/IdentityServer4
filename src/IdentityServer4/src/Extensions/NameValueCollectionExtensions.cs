// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.Encodings.Web;

namespace IdentityServer4.Extensions
{
    internal static class NameValueCollectionExtensions
    {
        private static readonly Dictionary<string, string> EmptyStringDictionary = new();
        private static readonly NameValueCollection EmptyNameValueCollection = new();

        public static IDictionary<string, string[]> ToFullDictionary(this NameValueCollection source)
        {
            return source.AllKeys.ToDictionary(k => k, k => source.GetValues(k));
        }

        public static NameValueCollection FromFullDictionary(this IDictionary<string, string[]> source)
        {
            var nvc = new NameValueCollection();
            foreach (var item in source)
            {
                foreach (var value in item.Value)
                {
                    nvc.Add(item.Key, value);
                }
            }

            return nvc;
        }
        
        public static string ToQueryString(this NameValueCollection collection)
        {
            if (collection.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(128);
            var first = true;
            foreach (string name in collection)
            {
                var values = collection.GetValues(name);
                if (values == null || values.Length == 0)
                {
                    first = AppendNameValuePair(builder, first, true, name, String.Empty);
                }
                else
                {
                    foreach (var value in values)
                    {
                        first = AppendNameValuePair(builder, first, true, name, value);
                    }
                }
            }

            return builder.ToString();
        }

        public static string ToFormPost(this NameValueCollection collection)
        {
            var builder = new StringBuilder(128);
            const string inputFieldFormat = "<input type='hidden' name='{0}' value='{1}' />\n";

            foreach (string name in collection)
            {
                var values = collection.GetValues(name);
                var value = values.First();
                value = HtmlEncoder.Default.Encode(value);
                builder.AppendFormat(inputFieldFormat, name, value);
            }

            return builder.ToString();
        }

        public static NameValueCollection ToNameValueCollection(this Dictionary<string, string>? data)
        {
            if (data is null || data.Count == 0)
            {
                return EmptyNameValueCollection;
            }

            var result = new NameValueCollection();
            foreach (var (key, value) in data)
            {
                if (value is not null)
                {
                    result.Add(key, value);
                }
            }

            return result;
        }

        public static Dictionary<string, string> ToScrubbedDictionary(this NameValueCollection? collection, params string[] nameFilter) =>
            collection.ToScrubbedDictionary(nameFilter.ToHashSet(StringComparer.OrdinalIgnoreCase));

        public static Dictionary<string, string> ToScrubbedDictionary(this NameValueCollection? collection, HashSet<string> nameFilter)
        {
            return collection?.AllKeys
                .Where(x => x is not null)
                .ToDictionary(
                    x => x, 
                    x => nameFilter.Contains(x) ? 
                            "***REDACTED***" :
                            collection.Get(x)) ??
                EmptyStringDictionary;
        }

        internal static string ConvertFormUrlEncodedSpacesToUrlEncodedSpaces(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Contains('+'))
            {
                str = str.Replace("+", "%20");
            }

            return str;
        }

        private static bool AppendNameValuePair(StringBuilder builder, bool first, bool urlEncode, string name, string value)
        {
            var effectiveName = name ?? String.Empty;
            var encodedName = urlEncode ? UrlEncoder.Default.Encode(effectiveName) : effectiveName;

            var effectiveValue = value ?? String.Empty;
            var encodedValue = urlEncode ? UrlEncoder.Default.Encode(effectiveValue) : effectiveValue;
            encodedValue = ConvertFormUrlEncodedSpacesToUrlEncodedSpaces(encodedValue);

            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append("&");
            }

            builder.Append(encodedName);
            if (!String.IsNullOrEmpty(encodedValue))
            {
                builder.Append("=");
                builder.Append(encodedValue);
            }
            
            return first;
        }
    }
}
