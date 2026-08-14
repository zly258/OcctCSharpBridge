param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$tracked = @(& git -C $RepositoryRoot ls-files)
if ($LASTEXITCODE -ne 0) { throw "Unable to inspect tracked demo sources." }

$forbiddenSdkSources = @(
    $tracked | Where-Object {
        $_ -like "src/OcctNative/*" -or
        $_ -like "src/OcctNet/*" -or
        $_ -like "src/OcctNet.WinForms/*" -or
        $_ -like "src/OcctNet.Wpf/*" -or
        $_ -like "src/OcctNet.Avalonia/*"
    }
)
if ($forbiddenSdkSources.Count -gt 0) {
    throw "demo-dev must consume the main SDK and must not track SDK implementation sources."
}

$sourceFiles = @(
    $tracked | Where-Object { $_ -like "src/*.cs" -or $_ -like "src/*/*.cs" }
)
$legacyPattern = '\bocct_(?:create|destroy|last_error|initialize|initialize_surface|model_create|model_destroy|model_last_error)\b|\b(?:NativeOcctSurface|LegacyNativeSurface)\b'
$violations = @()
foreach ($relativePath in $sourceFiles) {
    $path = Join-Path $RepositoryRoot $relativePath
    $matches = @(Select-String -LiteralPath $path -Pattern $legacyPattern -AllMatches)
    foreach ($match in $matches) {
        $violations += "${relativePath}:$($match.LineNumber)"
    }
}
if ($violations.Count -gt 0) {
    throw "Demo implementation uses legacy Bridge APIs: $($violations -join ', ')"
}

Write-Host "[consumer] Demo contains no SDK implementation sources or legacy lifecycle/surface calls." -ForegroundColor Green
