# IdentityServer4 fork — OpenID Connect and OAuth 2.0 for ASP.NET Core on .NET 10

[![CI](https://github.com/yartat/IdentityServer4/actions/workflows/ci.yml/badge.svg)](https://github.com/yartat/IdentityServer4/actions/workflows/ci.yml)

This repository is a maintained fork of [IdentityServer4](https://github.com/IdentityServer/IdentityServer4) 4.x, the
[OpenID Connect](https://openid.net/connect/) and [OAuth 2.0](https://datatracker.ietf.org/doc/html/rfc6749) framework for
ASP.NET Core created by [Dominick Baier](https://twitter.com/leastprivilege) and [Brock Allen](https://twitter.com/brocklallen).
It is maintained by [Yaroslav Tatarenko](https://github.com/yartat) and developed as a general-purpose library.

Upstream ended its free maintenance in November 2022. This fork keeps the IdentityServer4 programming model
(so existing applications migrate by changing a package reference and recompiling) and adds what a long-lived
deployment needs: a current .NET, current dependencies, security fixes and automatic signing key management.

> **Status:** the packages are **not published to nuget.org yet** (see [Packages](#packages)).
> Build them from source — see [Getting started](#getting-started). What changed in the first release: [RELEASE_NOTES.md](RELEASE_NOTES.md).

## Read this before you adopt it

- **Not certified.** IdentityServer4 was certified by the OpenID Foundation at specific upstream versions. This fork has not been
  re-certified; it is covered by its own automated tests (1,200 tests: unit, integration and EF Core store tests), not by conformance suites.
- **Not affiliated** with Duende Software, the .NET Foundation or the original authors, and **not commercially supported**.
  If you need a vendor-backed, certified product, use [Duende IdentityServer](https://duendesoftware.com/products/identityserver) or
  [OpenIddict](https://documentation.openiddict.com/). If you are happy to own an Apache-2.0 library, this fork is for you.
- **.NET 10 only** (`net10.0`). Earlier frameworks are not supported.
- Names and namespaces are still `IdentityServer4.*`, so the fork cannot be referenced together with the original
  `IdentityServer4` assemblies (see [RELEASE_NOTES.md](RELEASE_NOTES.md#known-limitations)).

## Packages

| Project | NuGet id | What it is |
|---|---|---|
| [`src/IdentityServer4`](src/IdentityServer4) | `OidcForge` | The framework: protocol endpoints, validators, stores, automatic key management |
| [`src/Storage`](src/Storage) | `OidcForge.Storage` | Models and store interfaces (for custom stores and UI layers) |
| [`src/EntityFramework.Storage`](src/EntityFramework.Storage) | `OidcForge.EntityFramework.Storage` | EF Core entities, DbContexts and stores |
| [`src/EntityFramework`](src/EntityFramework) | `OidcForge.EntityFramework` | `AddConfigurationStore` / `AddOperationalStore` for the framework |
| [`src/AspNetIdentity`](src/AspNetIdentity) | `OidcForge.AspNetIdentity` | ASP.NET Core Identity integration |

Namespaces and assembly names are unchanged (`IdentityServer4.*`). The packages are built by the [Release workflow](.github/workflows/release.yml);
see [.github/CONTRIBUTING.md](.github/CONTRIBUTING.md) for the release process.

## What is different from IdentityServer4 4.x

- **.NET 10**, ASP.NET Core / EF Core 10, Microsoft.IdentityModel 8.x; [Duende.IdentityModel](https://github.com/DuendeSoftware/foss) (Apache-2.0)
  instead of `IdentityModel`; `System.Text.Json` instead of Newtonsoft.Json; Mapster (generated code) instead of AutoMapper.
- **Automatic signing key management** (opt-in): keys are created, announced, rotated and retired for you; stored in files or in the
  EF operational store. See [docs/topics/crypto.rst](docs/topics/crypto.rst).
- **Security fixes** found in an audit of the fork: single-use authorization codes, strict `redirect_uri` matching,
  race-safe grant removal, refresh token reuse detection, no trust in client-supplied `X-Forwarded-For`, redaction of secrets in logs and more.
- Samples (Quickstarts, Clients) run on .NET 10 and reference the projects of this repository; the sample APIs expose OpenAPI and a Scalar UI.

The complete list, including breaking changes and a migration guide, is in [RELEASE_NOTES.md](RELEASE_NOTES.md).

## Getting started

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) (the exact version is pinned in [`global.json`](global.json)), then:

```powershell
git clone https://github.com/yartat/IdentityServer4.git
cd IdentityServer4
./build.ps1        # or ./build.sh — builds, runs all tests, packs to ./nuget
dotnet nuget add source ./nuget --name identityserver4-fork
```

A minimal token service (client credentials), as in [Quickstart 1](samples/Quickstarts/1_ClientCredentials):

```csharp
services.AddIdentityServer()
    .AddInMemoryApiScopes(new[] { new ApiScope("api1", "My API") })
    .AddInMemoryClients(new[]
    {
        new Client
        {
            ClientId = "client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedScopes = { "api1" }
        }
    })
    .AddDeveloperSigningCredential();   // development only

app.UseIdentityServer();
```

For production, let the framework manage the signing keys instead of `AddDeveloperSigningCredential`:

```csharp
services.AddIdentityServer(options => options.KeyManagement.Enabled = true);
```

### Build and test

```powershell
dotnet test src/IdentityServer4/test/IdentityServer.UnitTests
dotnet test src/IdentityServer4/test/IdentityServer.IntegrationTests
dotnet test src/EntityFramework.Storage/test/IntegrationTests
```

`IdentityServer4.Library.sln` builds the libraries; every area (`src/*`, `samples/*`) also has its own solution.
NuGet audit is on: a package with a known vulnerability fails the build.

## Samples

| Sample | What it shows |
|---|---|
| [`samples/Quickstarts`](samples/Quickstarts) | The six classic quickstarts: client credentials, interactive MVC, APIs, JavaScript client, EF Core stores, ASP.NET Core Identity |
| [`samples/Clients`](samples/Clients) | Console, MVC and API clients for every grant type |
| [`samples/KeyManagement`](samples/KeyManagement) | Automatic signing key management with file and database stores |

The sample APIs serve `/openapi/v1.json` and a [Scalar](https://scalar.com/) UI at `/scalar/v1` in the Development environment.
The HTTPS development certificate must be trusted (`dotnet dev-certs https --trust`).

## Documentation

The Sphinx sources are in [`docs/`](docs). The hosted site [identityserver4.readthedocs.io](https://identityserver4.readthedocs.io)
documents the upstream project; it is a good guide to the concepts, but where it differs from this fork
(target framework, key management, options listed in the release notes), this repository is authoritative.

## Security

Please report vulnerabilities privately — see [SECURITY.MD](SECURITY.MD). Do not open public issues for them.

## Contributing

Open an issue to discuss a change first, then send a pull request. Before you do: `./build.ps1` must pass (all tests green, no
vulnerable packages), and the security fixes listed in [RELEASE_NOTES.md](RELEASE_NOTES.md#security-fixes) must keep their regression tests.

## License and attribution

Licensed under [Apache 2.0](LICENSE). Copyright (c) Brock Allen & Dominick Baier for the original IdentityServer4 code;
modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork. Source files carry both notices.

Built with [ASP.NET Core](https://github.com/dotnet/aspnetcore), [Duende.IdentityModel](https://github.com/DuendeSoftware/foss),
[Mapster](https://github.com/MapsterMapper/Mapster), [Bullseye](https://github.com/adamralph/bullseye) and
[SimpleExec](https://github.com/adamralph/simple-exec).
