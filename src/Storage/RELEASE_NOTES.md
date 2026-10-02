# OidcForge.Storage — release notes

Release notes of the package `OidcForge.Storage` (original id `IdentityServer4.Storage`).
The overview of the whole release, the upgrade guide and the known limitations are in the
[release notes of the repository](../../RELEASE_NOTES.md). The baseline of the comparisons is IdentityServer4 4.1.2.

## 4.2.0

### Dependencies

| Dependency | Version |
|---|---|
| `Duende.IdentityModel` | 8.1.0 (replaces `IdentityModel`) |

Target framework: `net10.0` only (was `netcoreapp3.1` … `net6.0`). Newtonsoft.Json is no longer a dependency.

### Added

- **`ISigningKeyStore`** (`LoadKeysAsync`, `StoreKeyAsync`, `DeleteKeyAsync`) and the model **`SerializedKey`** (`Id`, `Version`, `Created`,
  `Algorithm`, `Data`, `DataProtected`): the persistence contract of automatic signing key management. The default store (files) is in `OidcForge`,
  the database store in `OidcForge.EntityFramework.Storage`.

### Carried over from the fork's 4.x-ideals builds

Already part of the `4.2.x-ideals` builds; relative to upstream 4.1.2 they are changes of the public API.

- `IPersistedGrantStore.GetAndRemoveAsync` and `IAuthorizationCodeStore.GetAndRemoveAuthorizationCodeAsync` / `StoreAuthorizationCodeAsync(handle, code)`.
  `GetAndRemoveAsync` **must be atomic**: for concurrent calls with the same key exactly one caller gets the grant, the others get `null`.
  This is what makes authorization codes single use.
- `Client.RedirectUris` and `Client.PostLogoutRedirectUris` are `ICollection<Uri>`; `Client.AllowedGrantTypes` is an `ISet<string>`.
- `RefreshToken.IpAddress` and `RefreshToken.Device`.

### Changed

- **Serialization uses `System.Text.Json`.** `PersistentGrantSerializer`, `ClaimConverter`, `ClaimsPrincipalConverter` and the `*Lite` types write the same JSON
  as the former Newtonsoft.Json implementation, so grants stored by IdentityServer4 4.x and by the `4.2.x-ideals` builds still load
  (compatibility tests in `IdentityServer.UnitTests`). Collections that the models initialize themselves are populated, not replaced
  (`[JsonObjectCreationHandling(Populate)]` on `AuthorizationCode`).
- **`Client.AllowedGrantTypes` validates every change**, not only the assignment of the whole collection: `client.AllowedGrantTypes.Add("implicit")`
  on a code flow client throws `InvalidOperationException` (`implicit` together with `authorization_code` or `hybrid` is illegal; so is
  `authorization_code` together with `hybrid`).

### Breaking changes

| Change | What to do |
|---|---|
| `net10.0` only | Retarget the application |
| `IdentityModel` → `Duende.IdentityModel` | Change the namespaces where your code uses its types |
| `CustomContractResolver` removed; `ClaimConverter` and `ClaimsPrincipalConverter` are `System.Text.Json` converters | Only relevant if you used these types; use `IPersistentGrantSerializer` |
| New interface members (see above) | Implement them in custom stores |
| `Client` collection types (see above) | `new Uri(...)` for redirect URIs; `ISet<string>` for grant types |

### Tests

The package has no test project of its own; it is covered by the unit tests of `OidcForge` (serializer compatibility, client validation,
grant types) and by the EF Core store tests.

### Upgrading

Replace `IdentityServer4.Storage` with `OidcForge.Storage` (normally it comes with `OidcForge`). A custom store needs the new members:

```csharp
public class MyGrantStore : IPersistedGrantStore
{
    // atomic: DELETE ... OUTPUT DELETED.* / DELETE ... RETURNING / a transaction with a row lock
    public Task<PersistedGrant> GetAndRemoveAsync(string key) { /* ... */ }
    // ... GetAsync, GetAllAsync, RemoveAsync, RemoveAllAsync, StoreAsync as before
}
```
