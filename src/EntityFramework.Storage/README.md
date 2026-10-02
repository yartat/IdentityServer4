# OidcForge.EntityFramework.Storage

Entity Framework Core persistence layer of the [OidcForge](https://github.com/yartat/IdentityServer4) OpenID Connect / OAuth 2.0
framework (a maintained fork of IdentityServer4 4.x): entities, DbContexts and store implementations for .NET 10 and EF Core 10.

```powershell
dotnet add package OidcForge.EntityFramework.Storage
```

Most applications use the wiring in [`OidcForge.EntityFramework`](https://www.nuget.org/packages/OidcForge.EntityFramework)
(`AddConfigurationStore`, `AddOperationalStore`) and do not reference this package directly. Take it when you need the entities or
DbContexts without the framework — for example in a migrations project or an admin UI.

## What is in it

- **`ConfigurationDbContext`** — clients, API resources, API scopes and identity resources (and their secrets, claims, properties).
- **`PersistedGrantDbContext`** — persisted grants (authorization codes, refresh and reference tokens, consent), device flow codes and
  **signing keys** (`Keys` table, used by automatic key management).
- **Stores** — `ClientStore`, `ResourceStore`, `PersistedGrantStore`, `DeviceFlowStore`, `SigningKeyStore`, plus the token cleanup service.
- **Mappers** — `ToModel()` / `ToEntity()` extension methods between entities and models. The mappers are generated code
  ([Mapster](https://github.com/MapsterMapper/Mapster)); there is no runtime reflection and no AutoMapper dependency.
- Table and schema names are configurable through `ConfigurationStoreOptions` / `OperationalStoreOptions`.

## Things that matter

- **Migrations.** This package ships no migrations: you own the schema. Create them in your own project
  (`dotnet ef migrations add … -c PersistedGrantDbContext`) — [reference migrations for SQL Server](https://github.com/yartat/IdentityServer4/tree/master/src/EntityFramework.Storage/migrations/SqlServer)
  are in the repository. If you upgrade from IdentityServer4 4.x, add a migration for the new `Keys` table. EF Core 9+ throws on
  `Migrate()` when the model has pending changes; check with `dotnet ef migrations has-pending-model-changes`.
- **Atomic grant redemption.** `PersistedGrantStore.GetAndRemoveAsync` deletes the row and reports a concurrency conflict as "not found",
  so of two concurrent requests for the same authorization code only one succeeds.
- **Token cleanup** (`EnableTokenCleanup`, `TokenCleanupInterval`, `TokenCleanupBatchSize`) removes expired grants and device codes.

## Status and license

Not certified, not affiliated with Duende Software or the .NET Foundation, no commercial support. See the
[release notes](https://github.com/yartat/IdentityServer4/blob/master/src/EntityFramework.Storage/RELEASE_NOTES.md) and the
[issue tracker](https://github.com/yartat/IdentityServer4/issues). Report vulnerabilities privately
([SECURITY.MD](https://github.com/yartat/IdentityServer4/blob/master/SECURITY.MD)).

Apache 2.0. Copyright (c) Brock Allen & Dominick Baier, Scott Brady; modifications copyright (c) Yaroslav Tatarenko.
