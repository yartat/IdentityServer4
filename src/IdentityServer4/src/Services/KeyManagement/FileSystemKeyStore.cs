// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// Stores signing keys as JSON files in a directory. Instances of a server farm can share a network directory.
    /// </summary>
    public class FileSystemKeyStore : ISigningKeyStore
    {
        private const string KeyFilePrefix = "is-signing-key-";
        private const string KeyFileExtension = ".json";

        private readonly DirectoryInfo _directory;
        private readonly ILogger<FileSystemKeyStore> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemKeyStore"/> class.
        /// </summary>
        /// <param name="path">The directory of the key files. It is created when the first key is stored.</param>
        /// <param name="logger">The logger.</param>
        public FileSystemKeyStore(string path, ILogger<FileSystemKeyStore> logger)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            _directory = new DirectoryInfo(path);
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SerializedKey>> LoadKeysAsync()
        {
            var keys = new List<SerializedKey>();

            _directory.Refresh();
            if (!_directory.Exists)
            {
                return keys;
            }

            foreach (var file in _directory.EnumerateFiles(KeyFilePrefix + "*" + KeyFileExtension))
            {
                try
                {
                    await using var stream = file.OpenRead();
                    var key = await JsonSerializer.DeserializeAsync<SerializedKey>(stream);
                    if (key != null)
                    {
                        keys.Add(key);
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is JsonException || ex is UnauthorizedAccessException)
                {
                    // e.g. deleted by another instance while the directory was enumerated
                    _logger.LogError(ex, "Failed to read signing key file {file}", file.FullName);
                }
            }

            return keys;
        }

        /// <inheritdoc/>
        public async Task StoreKeyAsync(SerializedKey key)
        {
            var path = GetPath(key.Id);
            _directory.Create();

            // write to a temporary file first, so other instances never read a partially written key
            var temporaryPath = path + ".tmp";
            await File.WriteAllTextAsync(temporaryPath, JsonSerializer.Serialize(key));
            File.Move(temporaryPath, path, overwrite: true);
        }

        /// <inheritdoc/>
        public Task DeleteKeyAsync(string id)
        {
            var path = GetPath(id);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return Task.CompletedTask;
        }

        private string GetPath(string id)
        {
            // key ids become file names
            if (string.IsNullOrEmpty(id) || !id.All(c => char.IsAsciiLetterOrDigit(c) || c == '-' || c == '_'))
            {
                throw new ArgumentException($"Invalid key id '{id}'.", nameof(id));
            }

            return Path.Combine(_directory.FullName, KeyFilePrefix + id + KeyFileExtension);
        }
    }
}
