# Checks the packages produced by build.ps1 / build.sh: every package of the family exists exactly once and carries its README.
#
#   verify-packages.ps1 [-Directory nuget] [-Version 1.0.0]
#
# Without -Version any version is accepted (CI builds are 1.0.0-ci.<run>); a release passes the exact version.
param(
    [string]$Directory = 'nuget',
    [string]$Version
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$ids = 'OidcForge', 'OidcForge.Storage', 'OidcForge.EntityFramework.Storage', 'OidcForge.EntityFramework', 'OidcForge.AspNetIdentity'

foreach ($id in $ids) {
    $escaped = [regex]::Escape($id)
    $versionPattern = if ($Version) { [regex]::Escape($Version) } else { '\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?' }
    $found = @(Get-ChildItem $Directory -Filter '*.nupkg' | Where-Object { $_.Name -match "^$escaped\.$versionPattern\.nupkg$" })
    if ($found.Count -ne 1) {
        throw "Expected exactly one $id package in '$Directory', found $($found.Count)."
    }

    $zip = [IO.Compression.ZipFile]::OpenRead($found[0].FullName)
    try {
        if (-not ($zip.Entries | Where-Object { $_.FullName -eq 'README.md' })) {
            throw "$($found[0].Name) does not contain README.md."
        }
    }
    finally {
        $zip.Dispose()
    }

    Write-Host "ok  $($found[0].Name)"
}
