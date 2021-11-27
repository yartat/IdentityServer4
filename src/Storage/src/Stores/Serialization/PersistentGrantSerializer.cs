// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Storage.Exceptions;
using IdentityServer4.Storage.Stores.Serialization;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdentityServer4.Stores.Serialization
{
    /// <summary>
    /// JSON-based persisted grant serializer
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.Serialization.IPersistentGrantSerializer" />
    public class PersistentGrantSerializer : IPersistentGrantSerializer
    {
        private static readonly JsonSerializerOptions Settings;

        private readonly PersistentGrantOptions? _options;
        private readonly IDataProtector? _provider;

        static PersistentGrantSerializer()
        {
            Settings = new JsonSerializerOptions
            {
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };

            Settings.Converters.Add(new ClaimConverter());
            Settings.Converters.Add(new ClaimsPrincipalConverter());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentGrantSerializer"/> class.
        /// </summary>
        /// <param name="options">The persist grant options</param>
        /// <param name="dataProtectionProvider">The data protection provider instance.</param>
        public PersistentGrantSerializer(PersistentGrantOptions? options = null, IDataProtectionProvider? dataProtectionProvider = null)
        {
            _options = options;
            _provider = dataProtectionProvider?.CreateProtector(nameof(PersistentGrantSerializer));
        }

        /// <summary>
        /// A value indicating whether data should be protected.
        /// </summary>
        internal bool ShouldProtect => _options?.ProtectData == true && _provider != null;

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string Serialize<T>(T value)
        {
            var payload = JsonSerializer.Serialize(value, Settings);

            if (ShouldProtect)
            {
                payload = _provider!.Protect(payload);
            }

            var data = new PersistentGrantLite
            {
                Version = 1,
                IsProtected = ShouldProtect,
                Payload = payload,
            };

            return JsonSerializer.Serialize(data, Settings);
        }

        /// <summary>
        /// Deserializes the specified string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public T? Deserialize<T>(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            var data = JsonSerializer.Deserialize<PersistentGrantLite>(json, Settings);
            if (data is null)
            {
                return default;
            }

            switch (data.Version)
            {
                case 0:
                    return JsonSerializer.Deserialize<T>(json, Settings);
                case 1:
                    var payload = data.Payload;
                    if (string.IsNullOrEmpty(payload))
                    {
                        return default;
                    }

                    if (data.IsProtected)
                    {
                        if (_provider is null)
                        {
                            throw new DataProtectException("Data protection provider has not been configured.");
                        }

                        payload = _provider.Unprotect(payload);
                    }

                    return JsonSerializer.Deserialize<T>(payload, Settings);
                default:
                    throw new InvalidPersistGrantVersionException($"Invalid version in the persisted grant: '{data.Version}'.");
            }
        }
    }
}