# OidcForge.EntityFramework

Adds Entity Framework Core persistence to the [OidcForge](https://github.com/yartat/IdentityServer4) OpenID Connect / OAuth 2.0 framework
(a maintained fork of IdentityServer4 4.x) on .NET 10: the configuration store (clients, resources), the operational store (grants,
device codes) and the store for automatically managed signing keys.

```powershell
dotnet add package OidcForge.EntityFramework
```

## Usage

```csharp
var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

services.AddIdentityServer(options => options.KeyManagement.Enabled = true)
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
            sql => sql.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
            sql => sql.MigrationsAssembly(migrationsAssembly));

        options.EnableTokenCleanup = true;       // remove expired grants in the background
    });
```

- `AddConfigurationStore()` registers the client and resource stores; `AddConfigurationStoreCache()` adds caching on top.
- `AddOperationalStore()` registers the grant and device flow stores, the token cleanup host and the **signing key store**:
  with `KeyManagement.Enabled` the keys live in the `Keys` table of the operational store instead of files.
  In a server farm share the ASP.NET Core data protection keys between all instances — the keys are protected with them.
- Use `AddConfigurationStore<TContext>()` / `AddOperationalStore<TContext>()` with your own DbContext types when you need to extend the model.

## Migrations

The packages do not ship migrations. Create them in your project:

```powershell
dotnet ef migrations add Grants -c PersistedGrantDbContext -o Migrations/PersistedGrantDb
dotnet ef migrations add Configuration -c ConfigurationDbContext -o Migrations/ConfigurationDb
```

**Upgrading from IdentityServer4 4.x:** the operational store has a new `Keys` table, so add a migration for `PersistedGrantDbContext`
and apply it before the first start. EF Core 9+ throws on `Migrate()` when the model has pending changes
(`dotnet ef migrations has-pending-model-changes -c PersistedGrantDbContext` tells you). Reference migrations for SQL Server:
[src/EntityFramework.Storage/migrations/SqlServer](https://github.com/yartat/IdentityServer4/tree/master/src/EntityFramework.Storage/migrations/SqlServer);
the [Quickstart 5](https://github.com/yartat/IdentityServer4/tree/master/samples/Quickstarts/5_EntityFramework) sample shows a complete setup.

## Status and license

Not certified, not affiliated with Duende Software or the .NET Foundation, no commercial support. See the
[release notes](https://github.com/yartat/IdentityServer4/blob/master/RELEASE_NOTES.md) and the
[issue tracker](https://github.com/yartat/IdentityServer4/issues). Report vulnerabilities privately
([SECURITY.MD](https://github.com/yartat/IdentityServer4/blob/master/SECURITY.MD)).

Apache 2.0. Copyright (c) Brock Allen & Dominick Baier, Scott Brady; modifications copyright (c) Yaroslav Tatarenko.
