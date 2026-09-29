// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Interface for the login URL processor
    /// </summary>
    public interface ILoginUrlProcessor
    {
        /// <summary>
        /// Processes a login URL.
        /// </summary>
        /// <param name="url">The login URL.</param>
        /// <param name="data">The login data.</param>
        /// <returns>The processed login URL.</returns>
        string Process(string url, IDictionary<string, string[]> data);
    }
}
