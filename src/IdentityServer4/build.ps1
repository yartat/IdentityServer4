$ErrorActionPreference = "Stop";
# fail on a non-zero exit code of dotnet (PowerShell 7.3+), not only on PowerShell errors
$PSNativeCommandUseErrorActionPreference = $true
dotnet run --project build -- $args