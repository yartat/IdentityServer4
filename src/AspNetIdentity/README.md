# OidcForge.AspNetIdentity

[ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity) integration for the
[OidcForge](https://github.com/yartat/IdentityServer4) OpenID Connect / OAuth 2.0 framework (a maintained fork of IdentityServer4 4.x)
on .NET 10: users, passwords and roles come from ASP.NET Core Identity; the framework issues the tokens.

```powershell
dotnet add package OidcForge.AspNetIdentity
```

## Usage

```csharp
services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));

services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

services.AddIdentityServer(options => options.KeyManagement.Enabled = true)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryClients(Config.Clients)
    .AddAspNetIdentity<ApplicationUser>();
```

`AddAspNetIdentity<TUser>()` registers:

- an `IProfileService` (`ProfileService<TUser>`) that turns the ASP.NET Core Identity user's claims and roles into token claims;
  the user id, user name and role claim types are mapped to `sub`, `name` and `role`,
- a resource owner password validator (`ResourceOwnerPasswordValidator<TUser>`) backed by `SignInManager<TUser>`; failed attempts count towards lockout,
- a security-stamp callback that keeps the claims captured at sign-in (`idp`, `auth_time`, `amr`) when ASP.NET Core Identity refreshes the principal,
- cookie configuration: the ASP.NET Core Identity application cookie becomes the default scheme and, like the external and
  two-factor cookies, is marked essential; the application and external cookies use `SameSite=None` (required for the iframe-based
  authorize and check-session requests); browsers accept `SameSite=None` cookies only over HTTPS.

Login, logout and consent pages are your application's UI — see the
[Quickstart 6](https://github.com/yartat/IdentityServer4/tree/master/samples/Quickstarts/6_AspNetIdentity) sample for a complete one.

## Notes

- `IdentityConstants` exists both in `Microsoft.AspNetCore.Identity` and in `IdentityServer4`; add a using alias
  (`using IdentityConstants = Microsoft.AspNetCore.Identity.IdentityConstants;`) when a file needs both.
- Only clients whose `AllowedGrantTypes` contain the `password` grant can use the password validator; prefer the authorization code flow with PKCE.

## Status and license

Not certified, not affiliated with Duende Software or the .NET Foundation, no commercial support. See the
[release notes](https://github.com/yartat/IdentityServer4/blob/master/src/AspNetIdentity/RELEASE_NOTES.md) and the
[issue tracker](https://github.com/yartat/IdentityServer4/issues). Report vulnerabilities privately
([SECURITY.MD](https://github.com/yartat/IdentityServer4/blob/master/SECURITY.MD)).

Apache 2.0. Copyright (c) Brock Allen & Dominick Baier; modifications copyright (c) Yaroslav Tatarenko.
