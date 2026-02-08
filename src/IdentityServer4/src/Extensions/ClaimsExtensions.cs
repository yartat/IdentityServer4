// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using IdentityModel;

namespace IdentityServer4.Extensions;

internal static class ClaimsExtensions
{
    private static readonly Dictionary<string, object> Empty = new();

    public static Dictionary<string, object> ToClaimsDictionary(this IEnumerable<Claim>? claims)
    {
        if (claims is null)
        {
            return Empty;
        }

        var result = new Dictionary<string, object>();
        var distinctClaims = claims.Distinct(new ClaimComparer());

        foreach (var claim in distinctClaims)
        {
            if (result.TryGetValue(claim.Type, out var existingValue))
            {
                if (existingValue is List<object> list)
                {
                    list.Add(GetValue(claim));
                }
                else
                {
                    result[claim.Type] = new List<object> { existingValue, GetValue(claim) };
                }
            }
            else
            {
                result.Add(claim.Type, GetValue(claim));
            }
        }

        return result;
    }

    private static object GetValue(Claim claim) => 
        claim.ValueType switch
        {
            ClaimValueTypes.Integer or ClaimValueTypes.Integer32 => ParseInt(claim.Value),
            ClaimValueTypes.Integer64 => ParseLong(claim.Value),
            ClaimValueTypes.Boolean => ParseBool(claim.Value),
            IdentityServerConstants.ClaimValueTypes.Json => ParseJson(claim.Value),
            _ => claim.Value
        };

    private static object ParseInt(string value) =>
        int.TryParse(value, out var result) ? result : (object)value;

    private static object ParseLong(string value) =>
        long.TryParse(value, out var result) ? result : (object)value;

    private static object ParseBool(string value) =>
        bool.TryParse(value, out var result) ? result : (object)value;

    private static object ParseJson(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<JsonElement>(value)!;
        }
        catch
        {
            return value;
        }
    }
}