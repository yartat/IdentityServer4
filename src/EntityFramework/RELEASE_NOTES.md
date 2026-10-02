# OidcForge.EntityFramework — release notes

Release notes of the package `OidcForge.EntityFramework` (original id `IdentityServer4.EntityFramework`).
The overview of the whole release, the upgrade guide and the known limitations are in the
[release notes of the repository](../../RELEASE_NOTES.md). The baseline of the comparisons is IdentityServer4 4.1.2.

## 4.2.0 (unreleased)

### Dependencies

| Dependency | Version |
|---|---|
| `OidcForge.EntityFramework.Storage` | 4.2.0 |
| `OidcForge` | 4.2.0 |

Target framework: `net10.0` only (was `netcoreapp3.1` … `net6.0`). EF Core 10.0.12 comes with `OidcForge.EntityFramework.Storage`.

### Added

- **`AddOperationalStore()` also registers the signing key store** (`SigningKeyStore`, through `AddSigningKeyStore<T>()` of the core package).
  With `IdentityServerOptions.KeyManagement.Enabled` the automatically managed signing keys are stored in the `Keys` table of the operational
  store instead of the default key directory. Nothing changes when key management is disabled.

### Changed

- The extension methods `AddConfigurationStore`, `AddConfigurationStoreCache`, `AddOperationalStore`, `AddOperationalStoreNotification`
  and their `TContext` overloads keep their signatures. `CorsPolicyService` and `TokenCleanupHost` are unchanged apart from the
  framework and dependency updates.

### Breaking changes

- `net10.0` only.
- **The operational store needs a migration** for the new `Keys` table, whether or not you use key management
  (EF Core 9+ throws on `Migrate()` when the model has pending changes). See [OidcForge.EntityFramework.Storage](../EntityFramework.Storage/RELEASE_NOTES.md#upgrading).

### Known limitations

- Tests: 6 (Windows) / 4 (Linux, macOS) in `IdentityServer4.EntityFramework.Tests`; the stores themselves are tested in `OidcForge.EntityFramework.Storage`.

### Upgrading

1. Replace `IdentityServer4.EntityFramework` with `OidcForge.EntityFramework` (all `OidcForge*` packages need the same version).
2. Add the `Keys` migration for `PersistedGrantDbContext` and apply it before the first start:

   ```powershell
   dotnet ef migrations add Keys -c PersistedGrantDbContext -o Migrations/PersistedGrantDb
   dotnet ef migrations has-pending-model-changes -c PersistedGrantDbContext   # must report no changes afterwards
   ```
3. Optionally enable `KeyManagement` — configure shared ASP.NET Core data protection keys first when you run several instances.
