// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace IdentityServer4.Extensions
{
    internal static class StringExtensions
    {
        [DebuggerStepThrough]
        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMissing(this string value) => string.IsNullOrWhiteSpace(value);

        [DebuggerStepThrough]
        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPresent(this string value) => !string.IsNullOrWhiteSpace(value);
    }
}