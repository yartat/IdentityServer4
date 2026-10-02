# OidcForge.AspNetIdentity — release notes

Release notes of the package `OidcForge.AspNetIdentity` (original id `IdentityServer4.AspNetIdentity`).
The overview of the whole release, the upgrade guide and the known limitations are in the
[release notes of the repository](../../RELEASE_NOTES.md). The baseline of the comparisons is IdentityServer4 4.1.2.

## 4.2.0

The package has **no API changes**: `AddAspNetIdentity<TUser>()`, `ProfileService<TUser>`, `ResourceOwnerPasswordValidator<TUser>`,
`UserClaimsFactory<TUser>` and `SecurityStampValidatorCallback` keep their signatures and behavior. Recompile against the new packages.

### Dependencies

| Dependency | Version |
|---|---|
| `OidcForge` | 4.2.0 |
| Framework reference `Microsoft.AspNetCore.App` (ASP.NET Core Identity comes from the shared framework) | 10.0 |

Target framework: `net10.0` only (was `netcoreapp3.1` … `net6.0`).

### Changed

- Uses `Duende.IdentityModel` (`JwtClaimTypes`, `OidcConstants`) instead of `IdentityModel`; the package no longer brings `IdentityModel` into your application.
- The source file aliases `IdentityConstants` to `Microsoft.AspNetCore.Identity.IdentityConstants`, because the core package has its own
  `IdentityServer4.IdentityConstants`. Do the same (`using IdentityConstants = Microsoft.AspNetCore.Identity.IdentityConstants;`) in your files that need both.
- Everything that changes in the core package applies to the sign-in flow you build on top of it: single-use authorization codes, strict redirect URI
  matching, refresh token reuse detection, the client IP taken from the connection. See [OidcForge](../IdentityServer4/RELEASE_NOTES.md).

### Breaking changes

- `net10.0` only; `IdentityModel` types in your own code move to `Duende.IdentityModel`.

### Known limitations

- The package has no test project of its own; it is built with the repository and exercised by the Quickstart 6 sample (`samples/Quickstarts/6_AspNetIdentity`),
  which must compile and run against it.

### Upgrading

Replace the package reference `IdentityServer4.AspNetIdentity` with `OidcForge.AspNetIdentity` (keep `OidcForge` and the EF packages at the same version) and recompile.
