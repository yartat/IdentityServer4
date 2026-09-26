// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace IdentityServer.IntegrationTests.Common
{
    internal static class JsonTestHelper
    {
        /// <summary>
        /// Parses a JSON object into plain CLR values that assertions can compare directly:
        /// string, long, double, bool, null, <see cref="List{T}"/> of object for arrays and
        /// <see cref="Dictionary{TKey, TValue}"/> of string to object for nested objects.
        /// </summary>
        public static Dictionary<string, object> ParseObject(string json)
        {
            using var document = JsonDocument.Parse(json);
            return (Dictionary<string, object>)ToClrValue(document.RootElement);
        }

        private static object ToClrValue(JsonElement element) => element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var integer) ? (object)integer : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ToClrValue).ToList(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ToClrValue(p.Value)),
            _ => throw new NotSupportedException($"Unexpected JSON value kind {element.ValueKind}")
        };
    }
}
