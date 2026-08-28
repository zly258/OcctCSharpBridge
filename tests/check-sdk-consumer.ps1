param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$tracked = @(& git -C $RepositoryRoot ls-files)
if ($LASTEXITCODE -ne 0) { throw "Unable to inspect tracked Demo sources." }

# Keep the source branch small and deterministic. Generated SDKs, build outputs, archives,
# runtime binaries, caches and accidental large files must never become tracked source.
$forbiddenTrackedPathPatterns = @(
    '^(?:dist|external|artifacts|build|publish|\.cache)/',
    '(?:^|/)(?:bin|obj|TestResults|coverage)/'
)
$forbiddenTrackedExtensions = @(
    '.dll', '.exe', '.so', '.dylib', '.pdb', '.ilk', '.exp', '.idb', '.tlog',
    '.zip', '.tar', '.tgz', '.7z', '.rar', '.nupkg', '.snupkg'
)
$maxTrackedFileBytes = 2MB
foreach ($relativePath in $tracked) {
    $normalized = ([string]$relativePath).Replace('\', '/')
    foreach ($pattern in $forbiddenTrackedPathPatterns) {
        if ($normalized -match $pattern) {
            throw "Generated/cache path must not be tracked by the Demo branch: $normalized"
        }
    }

    $extension = [System.IO.Path]::GetExtension($normalized).ToLowerInvariant()
    if ($extension -in $forbiddenTrackedExtensions -or $normalized.EndsWith('.tar.gz', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Generated binary/archive must not be tracked by the Demo branch: $normalized"
    }

    $fullPath = Join-Path $RepositoryRoot $relativePath
    if (Test-Path -LiteralPath $fullPath -PathType Leaf) {
        $length = (Get-Item -LiteralPath $fullPath).Length
        if ($length -gt $maxTrackedFileBytes) {
            throw "Tracked file exceeds the 2 MiB repository hygiene limit: $normalized ($length bytes). Move large generated assets to Release/Artifacts or explicitly redesign the repository policy before tracking them."
        }
    }
}

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
    $actual = Get-ProjectTargetFramework ([string]$entry.Key)
    if ($actual -ne [string]$entry.Value) {
        throw "Demo project '$($entry.Key)' targets '$actual'; expected '$($entry.Value)'."
    }
}

$runScriptPath = Join-Path $RepositoryRoot "run.ps1"
if (-not (Test-Path -LiteralPath $runScriptPath -PathType Leaf)) { throw "Windows Demo run script was not found." }
$runScript = Get-Content -LiteralPath $runScriptPath -Raw -Encoding UTF8
if ($runScript -match '(?m)Framework\s*=\s*\[string\]\$contract\.dotnet\.(?:targetFramework|desktopTargetFramework)') {
    throw "run.ps1 must not derive Demo output paths from the Bridge minimum TFM."
}
if (-not $runScript.Contains("Get-ProjectTargetFramework")) {
    throw "run.ps1 must resolve Demo runtime TFM from the Demo project contract."
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

# Windows synchronization clones/builds the selected Bridge source branch and consumes only dist/win-x64.
$syncScriptPath = Join-Path $RepositoryRoot "sync.ps1"
$syncBridgePath = Join-Path $RepositoryRoot "tools\sync-bridge.ps1"
foreach ($path in @($syncScriptPath, $syncBridgePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Windows Demo Bridge sync script was not found: $path" }
}
$syncScript = Get-Content -LiteralPath $syncScriptPath -Raw -Encoding UTF8
$syncBridge = Get-Content -LiteralPath $syncBridgePath -Raw -Encoding UTF8
foreach ($required in @(
    "`$BridgeBranch = 'main'",
    'SourceBranch = $BridgeBranch',
    'ForceRebuild = $true'
)) {
    if (-not $syncScript.Contains($required)) { throw "sync.ps1 lost required branch-driven behavior: $required" }
}
foreach ($required in @(
    "https://github.com/zly258/OcctCSharpBridge.git",
    "'clone'",
    "'fetch'",
    "'checkout'",
    "Target = 'dist'",
    "Configuration = 'Release'",
    "bridge-manifest.json",
    "sourceCommit"
)) {
    if (-not $syncBridge.Contains($required)) { throw "tools/sync-bridge.ps1 lost required source-build Binary SDK behavior: $required" }
}
foreach ($forbidden in @(
    "Target = 'sdk'",
    "Target = 'all'",
    'releases/latest',
    '*-win-x64-portable.zip'
)) {
    if ($syncBridge.Contains($forbidden)) { throw "Bridge sync must build dist Release from the selected branch: $forbidden" }
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
    throw "Demo must consume the Bridge SDK and must not track SDK implementation sources."
}

$consumerExtensions = @(".cs", ".xaml", ".axaml")
$sourceFiles = @(
    $tracked | Where-Object {
        $path = [string]$_
        $extension = [System.IO.Path]::GetExtension($path)
        $path.StartsWith("src/", [StringComparison]::Ordinal) -and $extension -in $consumerExtensions
    }
)

$guardPatterns = @(
    '\bocct_[A-Za-z0-9_]+\b|\b(?:NativeOcctSurface|LegacyNativeSurface)\b|(?:LibraryImport|DllImport)\s*\(\s*"OcctNative"',
    '\b(?:OcctHandle|OcctModelHandle|nativeAbiVersion|legacyAbi4Exports|compatibilityExtensions|plannedRemoval)\b|\bmodelOf\s*\(',
    '\bEngine\.(?:Objects|Shapes|Exists|GetShape|GetName|SetName)\b',
    '\bEngine\.(?:SetColor|SetTransparency|SetVisible|SetLineWidth|SetMaterial)\b',
    '\bEngine\.Display\b|\bEngine\.(?:MakeTextShape|MakeLengthAnnotationShape|MakeAngleAnnotationShape|MakeRadiusAnnotationShape|MakeDiameterAnnotationShape)\b',
    '\b(?:EngineInitialized|EnableDefaultInteraction|EnableRectangleSelection)\b'
)

$violations = @()
foreach ($relativePath in $sourceFiles) {
    $path = Join-Path $RepositoryRoot $relativePath
    foreach ($pattern in $guardPatterns) {
        foreach ($match in @(Select-String -LiteralPath $path -Pattern $pattern -AllMatches -CaseSensitive)) {
            $violations += "${relativePath}:$($match.LineNumber): $($match.Line.Trim())"
        }
    }
}
if ($violations.Count -gt 0) {
    throw "Demo crosses the Bridge 3 consumer boundary or uses retired APIs:`n - $($violations -join "`n - ")"
}

Write-Host "[consumer] Demo remains a Bridge 3/ABI5 binary consumer; SDK sync builds dist Release from the selected Bridge branch, external dependency paths are untracked, repository hygiene rejects generated/large tracked artifacts, and the Bridge full QA/window-smoke gate is not rerun." -ForegroundColor Green
