// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using IdentityServer4.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Extension methods for hashing strings
    /// </summary>
    public static class HashExtensions
    {
        /// <summary>
        /// Computes SHA256 hash for the specified string value in the input parameter.
        /// </summary>
        /// <param name="input">The string value to compute hash.</param>
        /// <remarks>
        /// If the input string is null or empty, an empty string is returned.
        /// </remarks>
        /// <returns>Returns the computed hash as a base64 encoded string or empty string.</returns>
        public static string Sha256(this string? input)
        {
            if (input.IsMissing()) 
            {
                return string.Empty;
            }

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Computes SHA256 hash for the specified byte array in the input parameter.
        /// </summary>
        /// <param name="input">The byte array value to compute hash.</param>
        /// <remarks>
        /// If the input byte array is null, null is returned.
        /// </remarks>
        /// <returns>Returns the computed hash as a byte array or null.</returns>
        public static byte[]? Sha256([NotNullIfNotNull(nameof(input))] this byte[]? input)
        {
            if (input is null)
            {
                return null;
            }

            using var sha = SHA256.Create();
            return sha.ComputeHash(input);
        }

        /// <summary>
        /// Computes SHA512 hash for the specified string value in the input parameter.
        /// </summary>
        /// <param name="input">The string value to compute hash.</param>
        /// <remarks>
        /// If the input string is null or empty, an empty string is returned.
        /// </remarks>
        /// <returns>Returns the computed hash as a base64 encoded string or empty string.</returns>
        public static string Sha512(this string? input)
        {
            if (input.IsMissing())
            {
                return string.Empty;
            }

            using var sha = SHA512.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}