param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$tracked = @(& git -C $RepositoryRoot ls-files)
if ($LASTEXITCODE -ne 0) { throw "Unable to inspect tracked demo sources." }

$forbiddenSdkRoots = @(
    "src/OcctNative/",
    "src/OcctNet/",
    "src/OcctNet.WinForms/",
    "src/OcctNet.Wpf/",
    "src/OcctNet.Avalonia/"
)
$forbiddenSdkSources = @(
    $tracked | Where-Object {
        $path = [string]$_
        @($forbiddenSdkRoots | Where-Object { $path.StartsWith($_, [StringComparison]::Ordinal) }).Count -gt 0
    }
)
if ($forbiddenSdkSources.Count -gt 0) {
    throw "demo-dev must consume the main SDK and must not track SDK implementation sources."
}

$sourceFiles = @(
    $tracked | Where-Object {
        $path = [string]$_
        $path.StartsWith("src/", [StringComparison]::Ordinal) -and
        [System.IO.Path]::GetExtension($path) -eq ".cs"
    }
)

# A demo consumer must never cross the managed SDK boundary and call OcctNative directly.
$nativeAbiPattern = '\bocct_[A-Za-z0-9_]+\b|\b(?:NativeOcctSurface|LegacyNativeSurface)\b|(?:LibraryImport|DllImport)\s*\(\s*"OcctNative"'

# Pre-ABI5 handles and flat/compatibility metadata are retired by the Bridge 3 contract.
$retiredAbiPattern = '\b(?:OcctHandle|OcctModelHandle|nativeAbiVersion|legacyAbi4Exports|compatibilityExtensions|plannedRemoval)\b|\bmodelOf\s*\('

# Bridge 3 retired the old object snapshots/re-hydration helpers and per-object appearance aliases.
$retiredObjectPattern = '\bEngine\.(?:Objects|Shapes|Exists|GetShape|GetName|SetName)\b'
$retiredAppearancePattern = '\bEngine\.(?:SetColor|SetTransparency|SetVisible|SetLineWidth|SetMaterial)\b'

# Modeling-to-viewer handoff and BRep annotation creation now have explicit Bridge 3 domains.
$retiredInteropPattern = '\bEngine\.Display\b|\bEngine\.(?:MakeTextShape|MakeLengthAnnotationShape|MakeAngleAnnotationShape|MakeRadiusAnnotationShape|MakeDiameterAnnotationShape)\b'

# Viewport consumers must use the generation-aware lifecycle and feature flags introduced by Bridge 3.
$retiredViewportPattern = '\b(?:EngineInitialized|EnableDefaultInteraction|EnableRectangleSelection)\b'

$guardPatterns = @(
    $nativeAbiPattern,
    $retiredAbiPattern,
    $retiredObjectPattern,
    $retiredAppearancePattern,
    $retiredInteropPattern,
    $retiredViewportPattern
)

$violations = @()
foreach ($relativePath in $sourceFiles) {
    $path = Join-Path $RepositoryRoot $relativePath
    foreach ($pattern in $guardPatterns) {
        $matches = @(Select-String -LiteralPath $path -Pattern $pattern -AllMatches -CaseSensitive)
        foreach ($match in $matches) {
            $violations += "${relativePath}:$($match.LineNumber): $($match.Line.Trim())"
        }
    }
}
if ($violations.Count -gt 0) {
    throw "Demo implementation crosses the Bridge 3 consumer boundary or uses retired APIs:`n - $($violations -join "`n - ")"
}

Write-Host "[consumer] Demo is a Bridge 3/ABI5 consumer only: no SDK implementation sources, direct native ABI calls, pre-ABI5 handles/metadata, retired managed APIs, or retired viewport lifecycle flags." -ForegroundColor Green
