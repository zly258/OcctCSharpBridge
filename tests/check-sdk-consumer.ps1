param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$tracked = @(& git -C $RepositoryRoot ls-files)
if ($LASTEXITCODE -ne 0) { throw "Unable to inspect tracked demo sources." }

function Get-ProjectTargetFramework {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Demo project was not found: $RelativePath" }
    try { [xml]$project = Get-Content -LiteralPath $path -Raw -Encoding UTF8 }
    catch { throw "Demo project is not valid XML: $RelativePath" }
    $node = $project.SelectSingleNode("/Project/PropertyGroup/TargetFramework[normalize-space(.) != '']")
    if ($null -eq $node) { throw "Demo project does not declare TargetFramework: $RelativePath" }
    return ([string]$node.InnerText).Trim()
}

$expectedDemoFrameworks = [ordered]@{
    "src/OcctDemo.Common/OcctDemo.Common.csproj" = "net10.0"
    "src/OcctDemo.WinForms/OcctDemo.WinForms.csproj" = "net10.0-windows"
    "src/OcctDemo.Wpf/OcctDemo.Wpf.csproj" = "net10.0-windows"
    "src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj" = "net10.0"
}
foreach ($entry in $expectedDemoFrameworks.GetEnumerator()) {
    $actualFramework = Get-ProjectTargetFramework ([string]$entry.Key)
    if ($actualFramework -ne [string]$entry.Value) {
        throw "Demo project '$($entry.Key)' targets '$actualFramework'; expected '$($entry.Value)'. Demo applications default to .NET 10 independently of the Bridge Binary SDK minimum TFM."
    }
}

$runScriptPath = Join-Path $RepositoryRoot "run.ps1"
if (-not (Test-Path -LiteralPath $runScriptPath -PathType Leaf)) { throw "Windows Demo run script was not found." }
$runScript = Get-Content -LiteralPath $runScriptPath -Raw -Encoding UTF8
if ($runScript -match '(?m)Framework\s*=\s*\[string\]\$contract\.dotnet\.(?:targetFramework|desktopTargetFramework)') {
    throw "run.ps1 must not derive Demo output/runtime paths from the Bridge Binary SDK target framework."
}
if (-not $runScript.Contains("Get-ProjectTargetFramework")) {
    throw "run.ps1 must resolve the Demo runtime TFM from the Demo project contract."
}

$buildScriptPath = Join-Path $RepositoryRoot "build.ps1"
if (-not (Test-Path -LiteralPath $buildScriptPath -PathType Leaf)) { throw "Windows Demo build script was not found." }
$buildScript = Get-Content -LiteralPath $buildScriptPath -Raw -Encoding UTF8
foreach ($token in @(
    '$script:DemoCoreTargetFramework = "net10.0"',
    '$script:DemoDesktopTargetFramework = "net10.0-windows"'
)) {
    if (-not $buildScript.Contains($token)) { throw "build.ps1 lost the explicit .NET 10 Demo TFM contract: $token" }
}

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

$consumerExtensions = @(".cs", ".xaml", ".axaml")
$sourceFiles = @(
    $tracked | Where-Object {
        $path = [string]$_
        $extension = [System.IO.Path]::GetExtension($path)
        $path.StartsWith("src/", [StringComparison]::Ordinal) -and
        $extension -in $consumerExtensions
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

Write-Host "[consumer] Demo is a Bridge 3/ABI5 consumer only; Demo TFM remains .NET 10, Bridge SDK TFM remains an independent .NET 8-10-compatible contract, and retired/native APIs are not used." -ForegroundColor Green
