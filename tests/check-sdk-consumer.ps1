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

# A demo consumer must never cross the managed SDK boundary and call OcctNative directly.
$nativeAbiPattern = '\bocct_[A-Za-z0-9_]+\b|\b(?:NativeOcctSurface|LegacyNativeSurface)\b|(?:LibraryImport|DllImport)\s*\(\s*"OcctNative"'

# Bridge 3 retired the old object snapshots/re-hydration helpers and per-object appearance aliases.
$retiredObjectPattern = '\bEngine\.(?:Objects|Shapes|Exists|GetShape|GetName|SetName)\b'
$retiredAppearancePattern = '\bEngine\.(?:SetColor|SetTransparency|SetVisible|SetLineWidth|SetMaterial)\b'

# Modeling-to-viewer handoff and BRep annotation creation now have explicit Bridge 3 domains.
$retiredInteropPattern = '\bEngine\.Display\b|\bEngine\.(?:MakeTextShape|MakeLengthAnnotationShape|MakeAngleAnnotationShape|MakeRadiusAnnotationShape|MakeDiameterAnnotationShape)\b'

$guardPatterns = @(
    $nativeAbiPattern,
    $retiredObjectPattern,
    $retiredAppearancePattern,
    $retiredInteropPattern
)

$violations = @()
foreach ($relativePath in $sourceFiles) {
    $path = Join-Path $RepositoryRoot $relativePath
    foreach ($pattern in $guardPatterns) {
        $matches = @(Select-String -LiteralPath $path -Pattern $pattern -AllMatches)
        foreach ($match in $matches) {
            $violations += "${relativePath}:$($match.LineNumber): $($match.Line.Trim())"
        }
    }
}
if ($violations.Count -gt 0) {
    throw "Demo implementation crosses the Bridge 3 consumer boundary or uses retired APIs:`n - $($violations -join "`n - ")"
}

Write-Host "[consumer] Demo is a Bridge 3/ABI5 consumer only: no SDK implementation sources, direct native ABI calls, or retired managed APIs." -ForegroundColor Green
