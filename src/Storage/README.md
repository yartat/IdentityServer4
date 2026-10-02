# OidcForge.Storage

Models and storage interfaces of the [OidcForge](https://github.com/yartat/IdentityServer4) OpenID Connect / OAuth 2.0 framework
(a maintained fork of IdentityServer4 4.x) for .NET 10.

```powershell
dotnet add package OidcForge.Storage
```

You need this package directly only when you **implement your own stores** or build a UI layer that works with the models,
without taking a dependency on the whole framework. The framework (`OidcForge`) and the EF Core stores reference it.

## What is in it

- **Models** — `Client`, `Secret`, `ApiResource`, `ApiScope`, `IdentityResource`, `PersistedGrant`, `AuthorizationCode`, `RefreshToken`,
  `Token`, `DeviceCode`, `Consent`, `SerializedKey` and the related enums and `GrantType` constants.
- **Store interfaces** — `IClientStore`, `IResourceStore`, `IPersistedGrantStore`, `IAuthorizationCodeStore`, `IRefreshTokenStore`,
  `IReferenceTokenStore`, `IUserConsentStore`, `IDeviceFlowStore`, and `ISigningKeyStore` for automatic signing key management.
- **Serialization** — `IPersistentGrantSerializer` and the default `System.Text.Json` implementation. It writes the same JSON as the
  former Newtonsoft.Json implementation, so grants stored by earlier versions still load.

Namespaces are unchanged (`IdentityServer4.Models`, `IdentityServer4.Stores`). Targets `net10.0`.

## Implementing a store

Two members of the store interfaces have semantics that matter for security:

```csharp
public class MyGrantStore : IPersistedGrantStore
{
    // Must be atomic: when called concurrently for the same key, exactly one caller gets the grant,
    // the others get null. Authorization codes are redeemed through it (RFC 6749 §4.1.2 — single use).
    public Task<PersistedGrant> GetAndRemoveAsync(string key) { /* DELETE … RETURNING / OUTPUT */ }

    // … the other members
}
```

- Redirect URIs of a `Client` are `Uri` values; `AllowedGrantTypes` validates every change (for example `implicit` together with
  `authorization_code` throws).
- `ISigningKeyStore` (`LoadKeysAsync`, `StoreKeyAsync`, `DeleteKeyAsync`) persists the keys created by automatic key management;
  register an implementation with `AddSigningKeyStore<T>()`. Key material in `SerializedKey.Data` is already protected when
  `DataProtected` is true.

## Status and license

Not certified, not affiliated with Duende Software or the .NET Foundation, no commercial support.
See the [release notes](https://github.com/yartat/IdentityServer4/blob/master/src/Storage/RELEASE_NOTES.md) and the
[issue tracker](https://github.com/yartat/IdentityServer4/issues). Report vulnerabilities privately
([SECURITY.MD](https://github.com/yartat/IdentityServer4/blob/master/SECURITY.MD)).

Apache 2.0. Copyright (c) Brock Allen & Dominick Baier; modifications copyright (c) Yaroslav Tatarenko.
