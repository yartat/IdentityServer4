// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4
{
    /// <summary>
    /// Base64url encoding (RFC 4648 §5, without padding).
    /// Replaces IdentityModel.Base64Url, which is not part of Duende.IdentityModel.
    /// </summary>
    internal static class Base64Url
    {
        public static string Encode(byte[] arg) => System.Buffers.Text.Base64Url.EncodeToString(arg);

        public static byte[] Decode(string arg) => System.Buffers.Text.Base64Url.DecodeFromChars(arg);
    }
}
