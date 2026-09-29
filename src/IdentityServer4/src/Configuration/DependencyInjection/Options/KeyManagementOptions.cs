// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Options for automatic management of signing keys.
    /// A new key is announced in the discovery document for <see cref="PropagationTime"/> before it is used for signing,
    /// signs tokens until it is <see cref="RotationInterval"/> old, and is kept for validation for another <see cref="RetentionDuration"/>.
    /// </summary>
    public class KeyManagementOptions
    {
        /// <summary>
        /// Enables automatic key management. Disabled by default; static keys added with AddSigningCredential take precedence for signing.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Signing algorithms keys are created for. The first one is the default for tokens that do not restrict the algorithm.
        /// Supported: RS256, RS384, RS512, PS256, PS384, PS512, ES256, ES384, ES512.
        /// </summary>
        public ICollection<string> SigningAlgorithms { get; set; } = new List<string> { SecurityAlgorithms.RsaSha256 };

        /// <summary>
        /// Size of new RSA keys in bits. Defaults to 2048.
        /// </summary>
        public int RsaKeySize { get; set; } = 2048;

        /// <summary>
        /// Age at which a key stops being used for signing. Defaults to 90 days.
        /// </summary>
        public TimeSpan RotationInterval { get; set; } = TimeSpan.FromDays(90);

        /// <summary>
        /// Time a new key is announced in the discovery document before it is used for signing, so clients and APIs can pick it up.
        /// Defaults to 14 days.
        /// </summary>
        public TimeSpan PropagationTime { get; set; } = TimeSpan.FromDays(14);

        /// <summary>
        /// Time a key is still announced after it stopped signing, so tokens it signed can be validated. Defaults to 14 days.
        /// </summary>
        public TimeSpan RetentionDuration { get; set; } = TimeSpan.FromDays(14);

        /// <summary>
        /// Whether keys past their retention are deleted from the store. Defaults to true.
        /// </summary>
        public bool DeleteRetiredKeys { get; set; } = true;

        /// <summary>
        /// Whether key material is protected with ASP.NET Core data protection before it is stored. Defaults to true.
        /// In a server farm the data protection keys must be shared by all instances.
        /// </summary>
        public bool DataProtectKeys { get; set; } = true;

        /// <summary>
        /// Directory of the default file system key store. Defaults to "keys" under the current directory.
        /// </summary>
        public string KeyPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "keys");

        /// <summary>
        /// Time loaded keys are cached before the store is read again. Must be shorter than <see cref="PropagationTime"/>,
        /// so keys created by other instances are announced before they are used. Defaults to 24 hours.
        /// </summary>
        public TimeSpan KeyCacheDuration { get; set; } = TimeSpan.FromHours(24);

        /// <summary>
        /// Time after a key was created during which <see cref="InitializationKeyCacheDuration"/> applies instead of
        /// <see cref="KeyCacheDuration"/>, so all instances quickly converge on newly created keys. Defaults to 5 minutes.
        /// </summary>
        public TimeSpan InitializationDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Cache duration while a key is younger than <see cref="InitializationDuration"/>. Defaults to 1 minute.
        /// </summary>
        public TimeSpan InitializationKeyCacheDuration { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Delay after a key was created because no usable key existed, before the store is read again.
        /// Lets instances that started at the same time agree on the same key. Defaults to 5 seconds.
        /// </summary>
        public TimeSpan InitializationSynchronizationDelay { get; set; } = TimeSpan.FromSeconds(5);

        internal static readonly string[] SupportedSigningAlgorithms =
        {
            SecurityAlgorithms.RsaSha256, SecurityAlgorithms.RsaSha384, SecurityAlgorithms.RsaSha512,
            SecurityAlgorithms.RsaSsaPssSha256, SecurityAlgorithms.RsaSsaPssSha384, SecurityAlgorithms.RsaSsaPssSha512,
            SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.EcdsaSha384, SecurityAlgorithms.EcdsaSha512
        };

        internal void Validate()
        {
            if (SigningAlgorithms == null || SigningAlgorithms.Count == 0)
            {
                throw new InvalidOperationException("KeyManagement: at least one signing algorithm is required.");
            }

            var unsupported = SigningAlgorithms.Where(x => !SupportedSigningAlgorithms.Contains(x)).ToArray();
            if (unsupported.Length > 0)
            {
                throw new InvalidOperationException($"KeyManagement: unsupported signing algorithms: {string.Join(", ", unsupported)}.");
            }

            if (SigningAlgorithms.Distinct().Count() != SigningAlgorithms.Count)
            {
                throw new InvalidOperationException("KeyManagement: signing algorithms must be unique.");
            }

            if (RsaKeySize < 2048)
            {
                throw new InvalidOperationException("KeyManagement: RsaKeySize must be at least 2048.");
            }

            if (PropagationTime <= TimeSpan.Zero || RetentionDuration < TimeSpan.Zero)
            {
                throw new InvalidOperationException("KeyManagement: PropagationTime must be positive and RetentionDuration must not be negative.");
            }

            if (RotationInterval <= PropagationTime)
            {
                throw new InvalidOperationException("KeyManagement: RotationInterval must be longer than PropagationTime.");
            }

            if (KeyCacheDuration >= PropagationTime || InitializationKeyCacheDuration >= PropagationTime)
            {
                throw new InvalidOperationException("KeyManagement: KeyCacheDuration and InitializationKeyCacheDuration must be shorter than PropagationTime.");
            }

            if (InitializationSynchronizationDelay < TimeSpan.Zero)
            {
                throw new InvalidOperationException("KeyManagement: InitializationSynchronizationDelay must not be negative.");
            }
        }
    }
}
