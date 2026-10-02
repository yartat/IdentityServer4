# Release notes

All notable changes of this fork of [IdentityServer4](https://github.com/IdentityServer/IdentityServer4).
The format follows [Keep a Changelog](https://keepachangelog.com/); versions follow [Semantic Versioning](https://semver.org/).

Packages are published as `OidcForge*`. The original ids were `IdentityServer4`, `IdentityServer4.Storage`,
`IdentityServer4.EntityFramework.Storage`, `IdentityServer4.EntityFramework` and `IdentityServer4.AspNetIdentity`;
namespaces and assembly names are unchanged.

Every package has its own release notes with the changes, the breaking changes and the dependencies of that package:

| Package | Release notes |
|---|---|
| `OidcForge` | [src/IdentityServer4/RELEASE_NOTES.md](src/IdentityServer4/RELEASE_NOTES.md) |
| `OidcForge.Storage` | [src/Storage/RELEASE_NOTES.md](src/Storage/RELEASE_NOTES.md) |
| `OidcForge.EntityFramework.Storage` | [src/EntityFramework.Storage/RELEASE_NOTES.md](src/EntityFramework.Storage/RELEASE_NOTES.md) |
| `OidcForge.EntityFramework` | [src/EntityFramework/RELEASE_NOTES.md](src/EntityFramework/RELEASE_NOTES.md) |
| `OidcForge.AspNetIdentity` | [src/AspNetIdentity/RELEASE_NOTES.md](src/AspNetIdentity/RELEASE_NOTES.md) |

## 4.2.0

First release of the OidcForge packages. The version continues the numbering of IdentityServer4 4.x and of the fork's own
`4.x-ideals` builds; because the package ids are new, 4.2.0 is the first version of these packages. All five packages have the same version.

Comparisons below use **IdentityServer4 4.1.2**, the last upstream 4.x, as the baseline. For breaking changes the last column says whether
the change was already part of the fork's `4.2.x-ideals` builds, so you can see what is new if you come from those.

All changes are verified by the test suite: 1,200 tests on Windows (835 unit, 298 integration, 67 EF Core store tests) and
1,243 on Linux, where the EF Core store tests also run against SQLite; all passing.

### What this release is built from

- **Included:** IdentityServer4 4.1.2 (merged into the fork on 2021-09-14) plus the fork's changes up to `4.2.6-ideals`
  (everything on `master`), the migration to .NET 10 and the changes listed below.
- **Not included:** the builds tagged `4.2.7`/`4.2.8-ideals` (commits `002d37ee`, `e4745067`, `5a79deed`, 2021-11 … 2022-02). They are not
  part of any branch of the repository: nullable reference type annotations, a different removal of Newtonsoft.Json with a versioned
  envelope for persisted grants, a different JWT payload creation and the removal of the GitHub lock-threads workflow. See
  [Known limitations](#known-limitations) and [Upgrading](#upgrading).

### Highlights

- **.NET 10** with current dependencies: ASP.NET Core / EF Core 10.0.12, Microsoft.IdentityModel 8.23, Duende.IdentityModel 8.1,
  `System.Text.Json`, Mapster.
- **Automatic signing key management** — opt-in, file or EF store, data-protected keys, rotation without downtime.
- **Security fixes** for the authorization code flow, redirect URI validation, refresh tokens, caching and logging
  ([details](#security-fixes)).
- **Drop-in migration** from IdentityServer4 4.x: same namespaces and programming model ([upgrade guide](#upgrading)).

### Added

- **Automatic key management** (`IdentityServerOptions.KeyManagement`, disabled by default).
  For every algorithm in `SigningAlgorithms` (default `RS256`; RS/PS/ES 256, 384, 512 are supported) a key is announced in the
  discovery document for `PropagationTime` (14 days) before it signs, signs until it is `RotationInterval` (90 days) old, is kept
  for validation for `RetentionDuration` (14 days) and is then deleted. Keys are protected with ASP.NET Core data protection.
  The default store is a directory (`KeyPath`); `AddOperationalStore` stores keys in the new `Keys` table instead; custom stores
  implement `ISigningKeyStore` and are registered with `AddSigningKeyStore<T>()`. Keys added with `AddSigningCredential` still
  sign first; all keys are published. See [docs/topics/crypto.rst](docs/topics/crypto.rst) and `samples/KeyManagement`.
- `Validation.AllowedRelativeRedirectUriOrigins` — origins on which a registered *relative* redirect URI (for example `/signin-oidc`) matches.
- `UserInteraction.AllowedReturnUrlOrigins` — origins on which an absolute return URL is accepted by `IsValidReturnUrl`.
- `Discovery.CacheDuration` (default 1 minute) — server-side cache lifetime of the discovery document and JWKS.
- `IPersistedGrantDbContext.Keys` and the `Keys` table (EF Core operational store).
- `DefaultRefreshTokenService.RevokeRefreshTokensOnReuseAsync` — overridable reaction to refresh token reuse.
- Samples run on .NET 10; the sample APIs expose OpenAPI (`/openapi/v1.json`) and a Scalar UI (`/scalar/v1`) in Development.

### Fork features carried over from the 4.x-ideals line

These were already part of the `4.2.x-ideals` builds; relative to upstream 4.1.2 they are additions or changes.

- `ip` and `device` claims in access tokens; refresh tokens record the client IP and device
  (the values are now sanitized — see [Security fixes](#security-fixes)).
- Atomic redemption of grants: `IPersistedGrantStore.GetAndRemoveAsync`, `IAuthorizationCodeStore.GetAndRemoveAuthorizationCodeAsync`
  (the EF implementation is now race-safe).
- Redirect URIs of a client are `Uri` values and `Client.AllowedGrantTypes` is an `ISet<string>`.
- Session cookies are issued `Secure`; `SameSite` fixes; `IUserSession` integration and a reworked authorize callback handler.
- Optimized data stores, discovery endpoints and decorators; faster complete-sign-in.
- `TokenValidationResult.RefreshTokenHandle`; `GetSignOutCalled` is public.
- Compatibility fixes for SAML and WS-Federation based external identity providers and for responses from external domains.
- Extended trace logging of authorize and token requests.

### Changed

- **Target framework is `net10.0` only.** The `netcoreapp3.1`, `net5.0` and `net6.0` targets are gone.
- **Dependencies:** `IdentityModel` → `Duende.IdentityModel` (Apache-2.0); Newtonsoft.Json removed in favor of `System.Text.Json`
  (persisted grants keep their JSON format, existing rows still deserialize — covered by compatibility tests); AutoMapper →
  Mapster with generated code (the mappers were internal; `ToModel()` / `ToEntity()` are unchanged).
- Discovery and JWKS responses are cached with an expiry instead of for the lifetime of the process.
- `DefaultKeyMaterialService` merges static and automatically managed keys.
- Build: NuGet audit fails the build on vulnerable packages; SDK pinned in `global.json`; `LangVersion` 14.0 (was `preview`);
  CI runs on `master` / `feature/**` with pinned actions and a read-only token; the PowerShell build scripts now fail on a failing `dotnet`.

### Breaking changes

| Area | Change | What to do | Already in 4.2.x-ideals |
|---|---|---|---|
| Framework | `net10.0` only | Retarget the application; install the .NET 10 SDK | no |
| Packages | New package ids; assembly and namespace names unchanged | Replace the package references ([map](#package-map)) | no |
| `IdentityModel` | Replaced by `Duende.IdentityModel` | `using IdentityModel;` → `using Duende.IdentityModel;` where your code uses its types | no |
| `Client.RedirectUris`, `Client.PostLogoutRedirectUris` | `ICollection<Uri>` (was `ICollection<string>`) | `new Uri("https://app/callback")`; relative: `new Uri("/signin-oidc", UriKind.Relative)` | yes |
| `Client.AllowedGrantTypes` | `ISet<string>` (was `ICollection<string>`) that validates every change | Adding an illegal combination now throws `InvalidOperationException` | type: yes, validation on change: no |
| Redirect URI matching | Exact string comparison, no fragments; relative registered URIs need `AllowedRelativeRedirectUriOrigins` | Register every URI exactly as clients send it | no |
| Custom `IPersistedGrantStore` / `IAuthorizationCodeStore` | New members: `GetAndRemoveAsync`, `GetAndRemoveAuthorizationCodeAsync`, `StoreAuthorizationCodeAsync(handle, code)` | Implement them (`GetAndRemoveAsync` must be atomic) | yes |
| EF Core operational store | New `Keys` table | Add a migration for `PersistedGrantDbContext` | no |
| Authorization codes | Single use, even after a failed exchange | Clients must not retry a failed exchange with the same code | no |
| Refresh tokens | Reuse of a consumed one-time token revokes all refresh tokens of the user for that client | Override `AcceptConsumedTokenAsync` for a grace window if your clients retry | no |
| Client IP | Taken from the connection only; `X-Forwarded-For` is ignored | Behind a reverse proxy call `UseForwardedHeaders` with `KnownProxies` / `KnownNetworks` | no |
| Custom `StrictRedirectUriValidator` | `StringCollectionContainsString` takes `IEnumerable<Uri>`; new `IsRegistered`; options constructor | Adapt overrides | no |
| `OidcForge.Storage` serialization | `CustomContractResolver` removed; `ClaimConverter` and `ClaimsPrincipalConverter` are `System.Text.Json` converters | Only relevant if you used these types | no |
| `TestUserStore` | A user without a password can no longer sign in | Give test users a password | yes |
| JSON claims | Claims of type `ClaimValueTypes.Json` are written as `JsonElement` | Only relevant if you post-process JWT payloads | no |
| Request objects | Invalid request objects (missing `client_id` …) return `invalid_request_object` | Adjust client error handling | no |

### Security fixes

- **Authorization codes are single use** (RFC 6749 §4.1.2, §10.5; RFC 7636 §4.6). A code was put back into the store after a failed
  validation (wrong client, redirect URI, PKCE verifier), which allowed unlimited `code_verifier` guessing within the code's lifetime.
- **Secrets are not logged.** `code` and `code_verifier` are redacted from token request logs; tokens and codes in Trace logs are obfuscated.
- **`redirect_uri` / `post_logout_redirect_uri`:** exact string comparison as OpenID Connect requires; requests with a fragment or
  a malformed value are rejected (they caused HTTP 500 on `end_session`); a registered relative URI no longer matches on *any* host.
- **Client configuration validation** rejects `javascript:`, `data:`, `file:` and the other dangerous prefixes; the check compared
  the URI scheme to the prefix with its colon and never matched.
- **Grant type combinations** (`implicit` with `authorization_code` or `hybrid`) are validated on every change of the collection.
- **Grant redemption is race-safe** in the EF store: only one of two concurrent requests gets the grant; the loser gets `null`, not HTTP 500.
- **Refresh token reuse detection** revokes the whole grant (RFC 9700 §4.14.2).
- **Client-supplied headers no longer end up in tokens:** `X-Forwarded-For` and `REMOTE_ADDR` are ignored for the `ip` claim;
  `device` keeps only printable ASCII, at most 200 characters.
- **Discovery and JWKS caches expire and are keyed by issuer and base URL.** Before, the first response lived for the whole
  process: rotated keys were never published and a request with a forged `Host` header could poison the document for everyone.
- **`IsValidReturnUrl`** accepts absolute URLs only on configured origins.
- **Request object validation** returns `invalid_request_object`; token endpoint failures are logged at Warning again.
- **Dependencies** with known advisories were replaced or updated (`System.Text.Json` 6.0.0, `AutoMapper` 10.1.1,
  `Microsoft.Extensions.Caching.Memory` 6.0.0, `System.IdentityModel.Tokens.Jwt` 6.14.1); client certificates are loaded with `X509CertificateLoader`.
- `GetSubjectId()` no longer throws for a principal without an identity.

### Known limitations

- **Names and assembly identity.** Assemblies and namespaces are still `IdentityServer4.*`, signed with the same key as upstream, and the assembly
  version is `4.2.0.0` — higher than upstream's last `4.1.2.0`. .NET therefore binds any assembly built against upstream IdentityServer4 4.x
  to this one, even though its API differs (see the breaking changes); such a mix fails at run time with `MissingMethodException` /
  `TypeLoadException`. Third-party packages that depend on the `IdentityServer4` NuGet id (for example `IdentityServer4.Contrib.*`)
  pull the original assemblies in — reference the fork's projects or rebuild such packages against it.
- **Builds `4.2.7` / `4.2.8-ideals`** store persisted grants in an envelope (`{"version":1,"protected":…,"payload":…}`, optionally data-protected).
  This release reads the plain JSON format of IdentityServer4 4.x and of `4.2.0` … `4.2.6-ideals` and cannot read such rows ([upgrade notes](#upgrading)).
- **Obsolete API.** `ISystemClock` (obsolete in ASP.NET Core) is still part of many constructors; the build reports about 60 warnings for it.
  Moving the public API to `TimeProvider` is a planned breaking change.
- **No nullable annotations** in the public API (they exist only in the tag-only `4.2.7`/`4.2.8-ideals` builds).
- **Not certified** by the OpenID Foundation and not commercially supported; conformance suites have not been run.
- The hosted documentation (identityserver4.readthedocs.io) describes upstream and lags behind the fork.

### Upgrading

#### From IdentityServer4 4.1.x (upstream)

1. Install the .NET 10 SDK and retarget the project to `net10.0`.
2. Replace the package references ([map](#package-map)). Namespaces stay the same; remove references to `IdentityModel` and switch to `Duende.IdentityModel`.
3. Fix compile errors: `Uri` instead of `string` for client redirect URIs, `ISet<string>` for grant types, new members in custom stores.
4. If you use the EF operational store, add the `Keys` migration and apply it before the first start:
   `dotnet ef migrations add Keys -c PersistedGrantDbContext`. Check with `dotnet ef migrations has-pending-model-changes -c PersistedGrantDbContext`
   — EF Core 9+ throws on `Migrate()` when the model has pending changes.
5. Behind a reverse proxy, add `app.UseForwardedHeaders(...)` with your proxies before `app.UseIdentityServer()`.
6. If clients use relative redirect URIs, list their origins in `Validation.AllowedRelativeRedirectUriOrigins`.
7. Optionally turn on `KeyManagement.Enabled`; configure shared data protection keys first when you run several instances.
8. Run your integration tests; pay attention to clients that retry a failed token request.

#### From the fork's own builds (`4.0.x` … `4.2.6-ideals`)

Steps 1–2 and 4–8 above apply; step 3 is limited to what [the last column](#breaking-changes) marks as new. Persisted grants and the database schema
of the configuration store are unchanged, so existing rows keep working; only the `Keys` table is new.

#### From `4.2.7` / `4.2.8-ideals`

As above, and: persisted grants written by these builds use the envelope format and cannot be read — users have to sign in again and
clients have to obtain new refresh tokens (or let the old grants expire while both versions run side by side). If grants were stored with
`PersistentGrantOptions.ProtectData`, they additionally depend on the data protection keys of that deployment. The JWT payload is created by
a different implementation in these builds; compare the tokens of your APIs after the upgrade.

### Package map

| Original id | Fork id |
|---|---|
| `IdentityServer4` | `OidcForge` |
| `IdentityServer4.Storage` | `OidcForge.Storage` |
| `IdentityServer4.EntityFramework.Storage` | `OidcForge.EntityFramework.Storage` |
| `IdentityServer4.EntityFramework` | `OidcForge.EntityFramework` |
| `IdentityServer4.AspNetIdentity` | `OidcForge.AspNetIdentity` |

### Credits

IdentityServer4 is copyright (c) Brock Allen & Dominick Baier and licensed under Apache 2.0, as is this fork.
Modifications copyright (c) Yaroslav Tatarenko.
