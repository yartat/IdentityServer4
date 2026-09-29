// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4.Models;
using IdentityServer4.Services.KeyManagement;
using Xunit;

namespace IdentityServer.UnitTests.Services.KeyManagement
{
    public sealed class FileSystemKeyStoreTests : IDisposable
    {
        private const string Category = "FileSystemKeyStore";

        private readonly string _path = Path.Combine(Path.GetTempPath(), "is4-keys-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_path)) Directory.Delete(_path, true);
        }

        private FileSystemKeyStore CreateSubject() => new FileSystemKeyStore(_path, TestLogger.Create<FileSystemKeyStore>());

        private static SerializedKey CreateKey(string id) => new SerializedKey
        {
            Version = 1,
            Id = id,
            Created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Algorithm = "RS256",
            Data = "data-" + id,
            DataProtected = true
        };

        [Fact]
        [Trait("Category", Category)]
        public async Task missing_directory_should_return_no_keys()
        {
            (await CreateSubject().LoadKeysAsync()).Should().BeEmpty();
            Directory.Exists(_path).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task stored_keys_should_be_loaded_by_another_instance()
        {
            await CreateSubject().StoreKeyAsync(CreateKey("a1"));
            await CreateSubject().StoreKeyAsync(CreateKey("b2"));

            var keys = (await CreateSubject().LoadKeysAsync()).OrderBy(x => x.Id).ToList();

            keys.Should().BeEquivalentTo(new[] { CreateKey("a1"), CreateKey("b2") });
            keys[0].Created.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task deleted_key_should_not_be_loaded()
        {
            var subject = CreateSubject();
            await subject.StoreKeyAsync(CreateKey("a1"));
            await subject.StoreKeyAsync(CreateKey("b2"));

            await subject.DeleteKeyAsync("a1");
            await subject.DeleteKeyAsync("missing");

            (await subject.LoadKeysAsync()).Select(x => x.Id).Should().Equal("b2");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task unrelated_and_invalid_files_should_be_ignored()
        {
            var subject = CreateSubject();
            await subject.StoreKeyAsync(CreateKey("a1"));
            await File.WriteAllTextAsync(Path.Combine(_path, "readme.txt"), "not a key");
            await File.WriteAllTextAsync(Path.Combine(_path, "is-signing-key-broken.json"), "{ not json");

            (await subject.LoadKeysAsync()).Select(x => x.Id).Should().Equal("a1");
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData("../evil")]
        [InlineData("a\\b")]
        [InlineData("")]
        public async Task key_ids_that_are_not_safe_file_names_should_be_rejected(string id)
        {
            Func<Task> act = () => CreateSubject().StoreKeyAsync(CreateKey(id));

            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
