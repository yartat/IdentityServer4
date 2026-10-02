# OidcForge

OpenID Connect and OAuth 2.0 framework for ASP.NET Core on .NET 10 — a maintained fork of
[IdentityServer4](https://github.com/IdentityServer/IdentityServer4) 4.x.

It implements the protocol endpoints (authorize, token, userinfo, introspection, revocation, device authorization, end session,
discovery and JWKS), the validators and stores behind them, and the extensibility points of IdentityServer4, so an application
written against IdentityServer4 4.x migrates by changing the package reference and recompiling.

```powershell
dotnet add package OidcForge
```

Targets `net10.0`. Namespaces and assembly names are unchanged (`IdentityServer4.*`).

## Quick start

```csharp
using IdentityServer4.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityServer(options => options.KeyManagement.Enabled = true)
    .AddInMemoryApiScopes(new[] { new ApiScope("api1", "My API") })
    .AddInMemoryClients(new[]
    {
        new Client
        {
            ClientId = "client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("secret".Sha256()) },   // use a real secret store in production
            AllowedScopes = { "api1" }
        }
    });

var app = builder.Build();
app.UseIdentityServer();
app.Run();
```

The token endpoint is `/connect/token`, discovery is at `/.well-known/openid-configuration`.
For a complete walk-through see the [Quickstarts](https://github.com/yartat/IdentityServer4/tree/master/samples/Quickstarts).

## Automatic signing key management

Signing keys are created, announced in the discovery document, used for signing, retired and deleted for you
(opt-in: `options.KeyManagement.Enabled = true`). Defaults: a new key is published 14 days before it signs, signs for 90 days,
and stays published for another 14 days.

```csharp
services.AddIdentityServer(options =>
{
    options.KeyManagement.Enabled = true;
    options.KeyManagement.SigningAlgorithms = new[] { "RS256", "ES256" };   // one key per algorithm
    options.KeyManagement.KeyPath = "/var/lib/myapp/keys";                  // default file store
});
```

- Keys are protected with ASP.NET Core data protection — in a server farm all instances must share the data protection keys.
- Store keys in the database instead of files with `OidcForge.EntityFramework` (`AddOperationalStore`), or implement `ISigningKeyStore`
  and register it with `AddSigningKeyStore<T>()`.
- Keys added with `AddSigningCredential` keep signing first; all keys are published.

## What to know when you migrate from IdentityServer4 4.x

- Client redirect URIs are `Uri` values, matched by exact string comparison; fragments are rejected.
- Authorization codes are single use; reuse of a consumed one-time refresh token revokes the user's refresh tokens for that client.
- The client IP (`ip` claim) comes from the connection. Behind a reverse proxy use `UseForwardedHeaders` with `KnownProxies`.
- Custom `IPersistedGrantStore` and `IAuthorizationCodeStore` implementations need the new atomic `GetAndRemove…` members.

The complete list with a step-by-step guide is in the
[release notes](https://github.com/yartat/IdentityServer4/blob/master/RELEASE_NOTES.md).

## Status

Not certified by the OpenID Foundation, not affiliated with Duende Software or the .NET Foundation, no commercial support.
Covered by 1,200 automated tests. Report vulnerabilities privately — see
[SECURITY.MD](https://github.com/yartat/IdentityServer4/blob/master/SECURITY.MD). Other issues:
[issue tracker](https://github.com/yartat/IdentityServer4/issues).

## License

Apache 2.0. Copyright (c) Brock Allen & Dominick Baier; modifications copyright (c) Yaroslav Tatarenko.
