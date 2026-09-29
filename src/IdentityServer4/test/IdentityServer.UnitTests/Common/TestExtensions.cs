// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;

namespace IdentityServer.UnitTests.Common
{
    internal static class TestExtensions
    {
        public static string Repeat(this string value, int count)
        {
            var parts = new string[count];
            return parts.Aggregate((x, y) => (x ?? value) + value);
        }
    }
}