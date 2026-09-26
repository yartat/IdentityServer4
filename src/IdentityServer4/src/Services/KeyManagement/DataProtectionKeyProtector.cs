// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using Microsoft.AspNetCore.DataProtection;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// Protects signing keys with ASP.NET Core data protection.
    /// </summary>
    public class DataProtectionKeyProtector : ISigningKeyProtector
    {
        private const string Purpose = "IdentityServer4.KeyManagement";

        /// <summary>
        /// Version of the serialization format written by this protector.
        /// </summary>
        public const int CurrentVersion = 1;

        private readonly IDataProtector _dataProtector;
        private readonly KeyManagementOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProtectionKeyProtector"/> class.
        /// </summary>
        public DataProtectionKeyProtector(IDataProtectionProvider dataProtectionProvider, IdentityServerOptions options)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(Purpose);
            _options = options.KeyManagement;
        }

        /// <inheritdoc/>
        public SerializedKey Protect(KeyContainer key)
        {
            var data = KeyMaterial.Serialize(key.Key, key.Algorithm);

            return new SerializedKey
            {
                Version = CurrentVersion,
                Id = key.Id,
                Created = key.Created,
                Algorithm = key.Algorithm,
                Data = _options.DataProtectKeys ? _dataProtector.Protect(data) : data,
                DataProtected = _options.DataProtectKeys
            };
        }

        /// <inheritdoc/>
        public KeyContainer Unprotect(SerializedKey key)
        {
            if (key.Version != CurrentVersion)
            {
                throw new InvalidOperationException($"Key {key.Id} has unsupported serialization version {key.Version}.");
            }

            var data = key.DataProtected ? _dataProtector.Unprotect(key.Data) : key.Data;
            return new KeyContainer(key.Id, key.Algorithm, key.Created, KeyMaterial.Deserialize(data, key.Id));
        }
    }
}
