// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdentityServer4.Stores.Serialization
{
    /// <summary>
    /// A claims principal converter.
    /// </summary>
    /// <seealso cref="JsonConverter{ClaimsPrincipal}"/>
    public class ClaimsPrincipalConverter : JsonConverter<ClaimsPrincipal?>
    {
        /// <inheritdoc/>
        public override ClaimsPrincipal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var source = JsonSerializer.Deserialize<ClaimsPrincipalLite>(ref reader, options);
            if (source == null)
            {
                return null;
            }

            var claims = source.Claims?.Select(x => new Claim(x.Type, x.Value ?? string.Empty, x.ValueType));
            var id = new ClaimsIdentity(claims, source.AuthenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, ClaimsPrincipal? value, JsonSerializerOptions options)
        {
            var target = new ClaimsPrincipalLite
            {
                AuthenticationType = value?.Identity?.AuthenticationType,
                Claims = value?.Claims?.Select(x =>
                    new ClaimLite
                    {
                        Type = x.Type,
                        Value = x.Value,
                        ValueType = x.ValueType != ClaimValueTypes.String ? x.ValueType : null
                    }
                ).ToArray()
            };

            JsonSerializer.Serialize(writer, target, options);
        }
    }
}
