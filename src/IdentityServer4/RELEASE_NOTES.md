# OidcForge — release notes

Release notes of the package `OidcForge` (original id `IdentityServer4`), the framework itself.
The overview of the whole release, the upgrade guide and the known limitations are in the
[release notes of the repository](../../RELEASE_NOTES.md). The baseline of the comparisons is IdentityServer4 4.1.2.

## 4.2.0 (unreleased)

### Dependencies

| Dependency | Version |
|---|---|
| `OidcForge.Storage` | 4.2.0 |
| `Duende.IdentityModel` | 8.1.0 (replaces `IdentityModel`) |
| `Microsoft.IdentityModel.Protocols.OpenIdConnect` | 8.23.0 |
| `Microsoft.AspNetCore.Authentication.OpenIdConnect` | 10.0.12 |
| Framework reference `Microsoft.AspNetCore.App` | 10.0 |

Target framework: `net10.0` only (was `netcoreapp3.1` … `net6.0`). Newtonsoft.Json and AutoMapper are no longer dependencies.

### Added

- **Automatic signing key management** — `IdentityServerOptions.KeyManagement` (`KeyManagementOptions`), disabled by default:

  | Option | Default | Meaning |
  |---|---|---|
  | `Enabled` | `false` | Turns key management on; static credentials of `AddSigningCredential` still sign first |
  | `SigningAlgorithms` | `RS256` | One key per algorithm (RS/PS/ES 256, 384, 512) |
  | `RsaKeySize` | 2048 | Size of new RSA keys |
  | `PropagationTime` | 14 days | A new key is published this long before it signs |
  | `RotationInterval` | 90 days | A key signs until it is this old |
  | `RetentionDuration` | 14 days | A key stays published for validation after it stopped signing |
  | `DeleteRetiredKeys` | `true` | Retired keys are deleted from the store |
  | `DataProtectKeys` | `true` | Key material is protected with ASP.NET Core data protection |
  | `KeyPath` | `./keys` | Directory of the default file store |
  | `KeyCacheDuration`, `InitializationDuration`, `InitializationKeyCacheDuration`, `InitializationSynchronizationDelay` | 24 h, 5 min, 1 min, 5 s | Cache and start-up behavior of server farms; the cache durations must stay shorter than `PropagationTime` |

  Types (namespace `IdentityServer4.Services.KeyManagement`): `IKeyManager` / `KeyManager`, `IAutomaticKeyManagerKeyStore`,
  `ISigningKeyProtector` / `DataProtectionKeyProtector`, `ISigningKeyStoreCache` / `InMemoryKeyStoreCache`, `FileSystemKeyStore`, `KeyContainer`.
  A custom store is registered with `AddSigningKeyStore<T>()` (`ISigningKeyStore` is in `OidcForge.Storage`).
  `DefaultKeyMaterialService` has a new constructor overload that takes the `IAutomaticKeyManagerKeyStore`.
- `ValidationOptions.AllowedRelativeRedirectUriOrigins` — origins on which a registered *relative* redirect URI matches (default: none).
- `UserInteractionOptions.AllowedReturnUrlOrigins` — origins on which `IsValidReturnUrl` accepts an absolute URL (the origin of `BaseUri` always is).
- `DiscoveryOptions.CacheDuration` (default 1 minute) — server-side cache lifetime of the discovery document and the JWKS.
- `DefaultRefreshTokenService.RevokeRefreshTokensOnReuseAsync` (protected virtual) — what happens when a consumed refresh token is presented again.
- `TimeProvider` is registered in DI (used by the key management); `StringExtensions.MaxDeviceLength`.

### Carried over from the fork's 4.x-ideals builds

`ip` and `device` claims in access tokens (the client IP and the user agent); `GetAndRemoveAuthorizationCodeAsync` in the token request validation;
`Secure` session cookies and `SameSite` fixes; `IUserSession` integration and the reworked authorize callback handler; optimized discovery endpoints,
stores and decorators; `TokenValidationResult.RefreshTokenHandle`; compatibility fixes for SAML / WS-Federation based external identity providers;
extended trace logging of authorize and token requests.

### Changed

- **Redirect URI validation** (`StrictRedirectUriValidator`, `StrictRedirectUriValidatorAppAuth`): exact string comparison (ordinal, ignoring case);
  a requested URI with a fragment, a relative or a malformed URI is invalid and never throws. A *registered* relative URI matches only on the origins of
  `AllowedRelativeRedirectUriOrigins`. New constructors that take `IdentityServerOptions`; new protected `IsRegistered`;
  `StringCollectionContainsString` takes `IEnumerable<Uri>`.
- **Client IP and device:** `HttpContextExtensions.GetRequestIp` takes the address of the connection; its parameter `tryUseXForwardHeader` defaults to `false`
  (it was `true`) and, when set, accepts only a valid IP address. `StringExtensions.GetDevice` keeps printable ASCII only, at most 200 characters.
- **Discovery and JWKS** responses are cached with an expiry (`Discovery.CacheDuration`) and keyed by issuer and base URL (they were cached for the
  lifetime of the process under a constant key).
- **JWT payloads** (`TokenExtensions.CreateJwtPayload`) are built for Microsoft.IdentityModel 8: JSON valued claims are `JsonElement`.
- **Logging:** `LoggingOptions.TokenRequestSensitiveValuesFilter` also redacts `code` and `code_verifier`; tokens and authorization codes in Trace logs are
  obfuscated; failures at the token endpoint are logged at Warning again.
- **Request objects:** failures of the request object validation (for example a missing `client_id`) return `invalid_request_object`.
- Client certificates are loaded with `X509CertificateLoader`; `GetSubjectId()` and `TryGetSubjectId()` are null-safe for a principal without an identity.

### Breaking changes

| Change | What to do |
|---|---|
| `net10.0` only | Retarget the application; install the .NET 10 SDK |
| `IdentityModel` → `Duende.IdentityModel` | Change the namespaces where your code uses its types |
| Authorization codes are single use, also after a failed exchange (a wrong `code_verifier` or `redirect_uri` consumes the code) | Clients must not retry a failed exchange with the same code |
| Reuse of a consumed one-time refresh token revokes all refresh tokens of the user for that client | Override `AcceptConsumedTokenAsync` if clients retry (a grace window) |
| The client IP comes from the connection; `X-Forwarded-For` is ignored | Behind a reverse proxy use `UseForwardedHeaders` with `KnownProxies` / `KnownNetworks` |
| Registered relative redirect URIs match only on `AllowedRelativeRedirectUriOrigins`; requested URIs with a fragment never match | Configure the origins; register the URIs exactly as clients send them |
| Custom `StrictRedirectUriValidator` | `StringCollectionContainsString(IEnumerable<Uri>, string)`; new `IsRegistered`; adapt overrides |
| `GetRequestIp(context, tryUseXForwardHeader)` defaults to `false` | Pass `true` explicitly if you really want the header |
| JSON valued claims (`ClaimValueTypes.Json`) are `JsonElement` in the JWT payload | Only relevant if you post-process payloads |
| `invalid_request_object` for failed request objects | Adjust the error handling of clients that send request objects |
| `TestUserStore`: a user without a password cannot sign in (already in the 4.2.x-ideals builds) | Give test users a password |

Breaking changes of the models and the stores (`Client`, `IPersistedGrantStore`, `IAuthorizationCodeStore`, serialization) are listed in the notes of
[OidcForge.Storage](../Storage/RELEASE_NOTES.md).

### Security fixes

- **Authorization codes** are removed before they are validated and never put back (RFC 6749 §4.1.2, §10.5; RFC 7636 §4.6). Before, a failed validation
  (wrong client, `redirect_uri`, PKCE verifier) restored the code, which allowed unlimited `code_verifier` guessing within its lifetime.
- **`code` and `code_verifier` are redacted** in token request logs; tokens and codes in Trace logs are obfuscated.
- **`redirect_uri` and `post_logout_redirect_uri`** are compared as strings as OpenID Connect requires; a fragment or a malformed value is rejected
  (it caused HTTP 500 on `end_session`); a registered relative URI no longer matches on any host.
- **Client configuration validation** rejects `javascript:`, `data:`, `file:` and the other dangerous prefixes of `ValidationOptions.InvalidRedirectUriPrefixes`
  (the check compared the URI scheme with the prefix including its colon and never matched).
- **Refresh token reuse detection** revokes the grant (RFC 9700 §4.14.2).
- **The client IP and device in tokens** cannot be forged with request headers, and the device cannot inject control characters or unbounded text into tokens and logs.
- **Discovery and JWKS caches** expire and are keyed by issuer and base URL: rotated keys are published, and a request with a forged `Host` header cannot poison the
  document for everyone.
- **`IsValidReturnUrl`** accepts absolute URLs only on configured origins.
- **Dependencies** with known advisories were replaced or updated (`System.Text.Json` 6.0.0, `AutoMapper` 10.1.1, `Microsoft.Extensions.Caching.Memory` 6.0.0,
  `System.IdentityModel.Tokens.Jwt` 6.14.1).

### Tests

835 unit tests and 298 integration tests (1 skipped), including regression tests for every fix above, the lifecycle of the signing keys over 400 days,
and a token signed with a managed key that is validated against the published JWKS.

### Upgrading

1. Replace the package reference `IdentityServer4` with `OidcForge`; fix the `IdentityModel` namespaces; retarget to `net10.0`.
2. Behind a reverse proxy add `app.UseForwardedHeaders(...)` before `app.UseIdentityServer()`:

   ```csharp
   builder.Services.Configure<ForwardedHeadersOptions>(o =>
   {
       o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
       o.KnownProxies.Add(IPAddress.Parse("10.0.0.10"));   // your proxy; or KnownNetworks
   });
   ```
3. Relative redirect URIs in client configurations need `options.Validation.AllowedRelativeRedirectUriOrigins.Add("https://app.example.com")`.
4. Key management is opt-in: `options.KeyManagement.Enabled = true` (with a shared data protection key ring when several instances run).
5. Run your integration tests — clients that retry a failed token request with the same authorization code now get `invalid_grant`.
