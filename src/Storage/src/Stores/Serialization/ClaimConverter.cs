// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdentityServer4.Stores.Serialization
{
    /// <summary>
    /// A claim converter.
    /// </summary>
    /// <seealso cref="JsonConverter{Claim}"/>
    public class ClaimConverter : JsonConverter<Claim?>
    {
        /// <inheritdoc/>
        public override Claim? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var source = JsonSerializer.Deserialize<ClaimLite>(ref reader, options);
            return source is null ?
                null :
                new Claim(source.Type, source.Value ?? string.Empty, source.ValueType);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Claim? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                return;
            }

            var target = new ClaimLite
            {
                Type = value.Type,
                Value = value.Value,
                ValueType = value.ValueType != ClaimValueTypes.String ? value.Value : null
            };

            JsonSerializer.Serialize(writer, target, options);
        }
    }
}