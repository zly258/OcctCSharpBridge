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
$legacyNativePattern = '\bocct_(?:create|destroy|last_error|initialize|initialize_surface|model_create|model_destroy|model_last_error)\b|\b(?:NativeOcctSurface|LegacyNativeSurface)\b'
$retiredManagedPattern = '\bEngine\.(?:Objects|Shapes|ShapeCount|Exists|GetShape|GetName|SetName|GetObjectKind)\b'
$violations = @()
foreach ($relativePath in $sourceFiles) {
    $path = Join-Path $RepositoryRoot $relativePath
    foreach ($pattern in @($legacyNativePattern, $retiredManagedPattern)) {
        $matches = @(Select-String -LiteralPath $path -Pattern $pattern -AllMatches)
        foreach ($match in $matches) {
            $violations += "${relativePath}:$($match.LineNumber): $($match.Line.Trim())"
        }
    }
}
if ($violations.Count -gt 0) {
    throw "Demo implementation uses retired Bridge APIs:`n - $($violations -join "`n - ")"
}

Write-Host "[consumer] Demo contains no SDK implementation sources, legacy native lifecycle calls, or retired managed object APIs." -ForegroundColor Green
