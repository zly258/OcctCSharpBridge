param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Project {
    param([string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) { throw "Project file was not found: $RelativePath" }
    return [xml](Get-Content $path -Raw -Encoding UTF8)
}

function Get-ProjectReferences {
    param([xml]$Project)
    return @($Project.SelectNodes('/Project/ItemGroup/ProjectReference') | ForEach-Object { [string]$_.GetAttribute('Include') } | Where-Object { $_ })
}

function Get-PackageReferences {
    param([xml]$Project)
    return @($Project.SelectNodes('/Project/ItemGroup/PackageReference') | ForEach-Object { [string]$_.GetAttribute('Include') } | Where-Object { $_ })
}

function Assert-Reference {
    param([string[]]$References, [string]$Expected, [string]$ProjectName)
    $normalizedExpected = $Expected.Replace('/', '\')
    $matches = @($References | Where-Object { $_.Replace('/', '\') -eq $normalizedExpected })
    if ($matches.Count -ne 1) { throw "$ProjectName must reference exactly once: $Expected" }
}

function Test-TrackedPath {
    param([string]$RelativePath)
    $normalized = $RelativePath.Replace('\', '/')
    $tracked = @(& git -C $RepositoryRoot ls-files -- $normalized "$normalized/**" 2>$null)
    if ($LASTEXITCODE -ne 0) { throw "Unable to inspect tracked repository paths with git ls-files." }
    return $tracked.Count -gt 0
}

$core = Read-Project "src/OcctNet/OcctNet.csproj"
$avalonia = Read-Project "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj"
$coreProjectReferences = @(Get-ProjectReferences $core)
$corePackageReferences = @(Get-PackageReferences $core)
if ($coreProjectReferences.Count -ne 0) { throw "OcctNet core must not depend on UI host projects." }
foreach ($uiDependency in @("Avalonia", "PresentationFramework", "System.Windows.Forms")) {
    if ($uiDependency -in $corePackageReferences) { throw "OcctNet core must remain UI-framework independent: $uiDependency" }
}

$avaloniaReferences = @(Get-ProjectReferences $avalonia)
Assert-Reference $avaloniaReferences "..\OcctNet\OcctNet.csproj" "OcctNet.Avalonia"
if ($avaloniaReferences.Count -ne 1) { throw "OcctNet.Avalonia must depend only on OcctNet." }
$avaloniaPackages = @(Get-PackageReferences $avalonia)
if ("Avalonia" -notin $avaloniaPackages) { throw "OcctNet.Avalonia must reference Avalonia." }

foreach ($forbiddenPath in @(
    "src/OcctNet.WinForms",
    "src/OcctNet.Wpf",
    "src/OcctDemo.Common",
    "src/OcctDemo.WinForms",
    "src/OcctDemo.Wpf",
    "src/OcctDemo.Avalonia",
    "tests/OcctNet.X11Smoke",
    "dist",
    "publish.ps1",
    "publish.sh",
    "sync.ps1",
    "sync-dist.ps1"
)) {
    if (Test-TrackedPath $forbiddenPath) { throw "Standalone Avalonia branch must not track: $forbiddenPath" }
}

foreach ($legacyProject in @("src/CadCommon", "src/CadWinForms", "src/CadWpf", "src/CadAvalonia")) {
    if (Test-TrackedPath $legacyProject) { throw "Legacy application project must not be tracked: $legacyProject" }
}

$managedRoot = Join-Path $RepositoryRoot "src\OcctNet"
$managedText = (Get-ChildItem $managedRoot -Filter '*.cs' -File -Recurse | ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"
foreach ($forbidden in @("DocumentManager", "CommandBus", "CommandRegistry", "ToolManager")) {
    if ($managedText -match "\b$([regex]::Escape($forbidden))\b") { throw "Application-layer type must not enter OcctNet core: $forbidden" }
}

foreach ($legacyFile in @(
    "src/OcctNet/OcctObject.Legacy.cs",
    "src/OcctNet/OcctGeometryExtensions.Compatibility.cs",
    "src/OcctNet/OcctEngine.ApiAliases.cs",
    "src/OcctNet/NativeMethods.Modeling.cs"
)) {
    if (Test-TrackedPath $legacyFile) { throw "Legacy/compatibility source must not be reintroduced: $legacyFile" }
}

Write-Host "[architecture] Source-only OcctNet + OcctNet.Avalonia boundary validated for Windows/Linux." -ForegroundColor Green
