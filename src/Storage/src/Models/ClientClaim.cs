// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Claims;

namespace IdentityServer4.Models
{
    /// <summary>
    /// A client claim
    /// </summary>
    public class ClientClaim
    {
        /// <summary>
        /// The claim type
        /// </summary>
        public string Type { get; set; } = default!;

        /// <summary>
        /// The claim value
        /// </summary>
        public string Value { get; set; } = default!;

        /// <summary>
        /// The claim value type
        /// </summary>
        public string ValueType { get; set; } = ClaimValueTypes.String;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientClaim"/> class.
        /// </summary>
        public ClientClaim()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientClaim"/> class with type and value.
        /// </summary>
        /// <param name="type">The claim type</param>
        /// <param name="value">The claim value</param>
        public ClientClaim(string type, string value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientClaim"/> class with type, value and value type.
        /// </summary>
        /// <param name="type">The claim type</param>
        /// <param name="value">The claim value</param>
        /// <param name="valueType">The claim value type</param>
        public ClientClaim(string type, string value, string? valueType)
        {
            Type = type;
            Value = value;
            ValueType = valueType ?? ClaimValueTypes.String;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                hash = (hash * 23) + Value.GetHashCode();
                hash = (hash * 23) + Type.GetHashCode();
                hash = (hash * 23) + ValueType.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is not null &&
                obj is ClientClaim c &&
                string.Equals(Type, c.Type, StringComparison.Ordinal) &&
                string.Equals(Value, c.Value, StringComparison.Ordinal) &&
                string.Equals(ValueType, c.ValueType, StringComparison.Ordinal);
    }
}