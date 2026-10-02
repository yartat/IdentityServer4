# Contributing

Thank you for helping. The project is maintained on a best-effort basis; the fastest way to get a change in is a small, focused pull request
with tests. For anything larger, discuss it first (an issue or a draft pull request).

Contributions are licensed under [Apache 2.0](../LICENSE), like the project itself. There is no CLA.

## Before you send a pull request

- `./build.ps1` (or `./build.sh`) must pass: it builds, runs **all** tests and packs. NuGet audit is on — a package with a known
  vulnerability fails the build.
- Add tests for what you change. Security-relevant behavior (see "Security fixes" in [RELEASE_NOTES.md](../RELEASE_NOTES.md)) keeps its regression tests.
- Add an entry to [RELEASE_NOTES.md](../RELEASE_NOTES.md) under the unreleased version; a breaking change also gets a row in the table of breaking changes.
- New `.cs` files start with the fork header; files taken from upstream keep their header plus the modification line:

  ```
  // Copyright (c) Yaroslav Tatarenko. All rights reserved.
  // Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
  // Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
  ```
- Code samples in READMEs must compile.
- Report vulnerabilities privately — see [SECURITY.MD](../SECURITY.MD).

## What the CI does

| Event | Workflow | Result |
|---|---|---|
| Push to `master` or `feature/**`, pull request to `master` | [`ci.yml`](workflows/ci.yml) | Build, all tests and packaging on Linux, Windows and macOS; packages `4.2.0-ci.<run>` of the Windows build are attached to the run for 7 days |
| Tag `vMAJOR.MINOR.PATCH[-prerelease]` | [`release.yml`](workflows/release.yml) | Build, tests, packaging; publishing to nuget.org after a manual approval; GitHub release with the release notes |
| Manual run of `release.yml` | [`release.yml`](workflows/release.yml) | Dry run: builds `X.Y.Z-manual.<run>` packages, publishes nothing |

## Releasing (maintainers)

One-time setup in the repository settings:

1. **Environments → New environment `nuget`**: add yourself as a required reviewer (this is the approval gate in front of nuget.org)
   and the secret `NUGET_API_KEY` — a nuget.org API key with the *Push* scope, restricted to the glob pattern `OidcForge*`.
2. Optionally reserve the `OidcForge` ID prefix on nuget.org: there is no form, send an email to `account@nuget.org` with your nuget.org
   display name and the prefix ([criteria and process](https://learn.microsoft.com/nuget/nuget-org/id-prefix-reservation)). Do it after the first
   release, so the reviewers see packages that clearly belong to you; the package ids themselves are taken at the first push.
3. Enable *Private vulnerability reporting* (Security) and, if you want issues, *Issues* (Settings → Features).

For every release:

1. Set `VersionPrefix` in [`Directory.Build.props`](../Directory.Build.props) if the version changes.
2. In [`RELEASE_NOTES.md`](../RELEASE_NOTES.md) **and in the `RELEASE_NOTES.md` of every package** (`src/<area>/RELEASE_NOTES.md`) replace
   `(unreleased)` in the heading of the version with the date (`## 4.2.0 - 2026-10-15`). The release workflow refuses notes that are still
   marked unreleased or missing in any of the six files.
3. Merge to `master` and wait for a green CI run.
4. Optional dry run: *Actions → Release → Run workflow* on `master`; download the `packages` artifact and try it from a local feed
   (`dotnet nuget add source <folder>`).
5. Tag the commit on `master` and push the tag:

   ```powershell
   git tag v4.2.0
   git push origin v4.2.0
   ```

   A pre-release is `v4.2.0-rc.1` (the number before the dash must equal `VersionPrefix`). Tags are prefixed with `v`; the old bare tags (`4.2.0-ideals` … `4.2.8-ideals`) belong to the fork's earlier builds and are not release tags. The workflow fails when the tag does not
   match `VersionPrefix`, is not on `master`, or has no release notes.
6. Approve the **publish** job. After it succeeds the workflow creates the GitHub release with the packages and the release notes attached.
7. Open the next section in `RELEASE_NOTES.md` (`## 4.2.1 (unreleased)`) and bump `VersionPrefix`.

A published NuGet version cannot be changed or re-published. If something is wrong, unlist it on nuget.org and release the next patch version.
If only the publish job failed (for example a network error), re-run the failed jobs: pushing is idempotent (`--skip-duplicate`).
