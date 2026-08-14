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

function Get-PropertyValue {
    param([xml]$Project, [string]$Name)
    $node = $Project.SelectSingleNode("/Project/PropertyGroup/$Name")
    if ($null -eq $node) { return "" }
    return [string]$node.InnerText
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

function Get-TrackedSourceText {
    param([string[]]$RelativeRoots)

    $tracked = @(& git -C $RepositoryRoot ls-files 2>$null)
    if ($LASTEXITCODE -ne 0) { throw "Unable to inspect tracked repository sources with git ls-files." }

    $roots = @($RelativeRoots | ForEach-Object { $_.Replace('\', '/').TrimEnd('/') + '/' })
    $builder = [System.Text.StringBuilder]::new()
    foreach ($relativePath in $tracked) {
        $normalizedPath = ([string]$relativePath).Replace('\', '/')
        if (-not $normalizedPath.EndsWith('.cs', [System.StringComparison]::OrdinalIgnoreCase)) { continue }

        $underRoot = $false
        foreach ($root in $roots) {
            if ($normalizedPath.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
                $underRoot = $true
                break
            }
        }
        if (-not $underRoot) { continue }

        $path = Join-Path $RepositoryRoot $relativePath
        [void]$builder.AppendLine([System.IO.File]::ReadAllText($path))
    }
    return $builder.ToString()
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

$demoCommon = Read-Project "src/OcctDemo.Common/OcctDemo.Common.csproj"
if ((Get-PropertyValue $demoCommon "TargetFramework") -ne "net10.0") { throw "OcctDemo.Common must target net10.0 for Windows/Linux reuse." }
$demoCommonReferences = @(Get-ProjectReferences $demoCommon)
Assert-Reference $demoCommonReferences "..\OcctNet\OcctNet.csproj" "OcctDemo.Common"
if ($demoCommonReferences.Count -ne 1) { throw "OcctDemo.Common must depend only on OcctNet." }

$demoAvalonia = Read-Project "src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"
if ((Get-PropertyValue $demoAvalonia "TargetFramework") -ne "net10.0") { throw "OcctDemo.Avalonia must target net10.0 for Windows/Linux." }
if (-not [string]::IsNullOrWhiteSpace((Get-PropertyValue $demoAvalonia "UseWindowsForms"))) { throw "OcctDemo.Avalonia must not enable UseWindowsForms." }

$manifestNode = $demoAvalonia.SelectSingleNode('/Project/PropertyGroup/ApplicationManifest')
if ($null -eq $manifestNode -or [string]$manifestNode.InnerText -ne 'app.manifest') {
    throw "OcctDemo.Avalonia must embed app.manifest for Windows NativeControlHost support."
}
$manifestCondition = [string]$manifestNode.GetAttribute('Condition')
if (-not $manifestCondition.Contains("IsOSPlatform('Windows')", [System.StringComparison]::Ordinal)) {
    throw "OcctDemo.Avalonia ApplicationManifest must be conditioned to Windows only."
}
$manifestRelativePath = "src/OcctDemo.Avalonia/app.manifest"
if (-not (Test-TrackedPath $manifestRelativePath)) { throw "OcctDemo.Avalonia must track app.manifest for Windows native hosting." }
$manifestPath = Join-Path $RepositoryRoot $manifestRelativePath
$manifestText = Get-Content $manifestPath -Raw -Encoding UTF8
if (-not $manifestText.Contains('{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}', [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OcctDemo.Avalonia app.manifest must declare Windows 10/11 supportedOS compatibility."
}

$demoAvaloniaReferences = @(Get-ProjectReferences $demoAvalonia)
Assert-Reference $demoAvaloniaReferences "..\OcctDemo.Common\OcctDemo.Common.csproj" "OcctDemo.Avalonia"
Assert-Reference $demoAvaloniaReferences "..\OcctNet\OcctNet.csproj" "OcctDemo.Avalonia"
Assert-Reference $demoAvaloniaReferences "..\OcctNet.Avalonia\OcctNet.Avalonia.csproj" "OcctDemo.Avalonia"
if ($demoAvaloniaReferences.Count -ne 3) { throw "OcctDemo.Avalonia must depend only on Demo.Common, OcctNet, and OcctNet.Avalonia." }
$demoAvaloniaPackages = @(Get-PackageReferences $demoAvalonia)
foreach ($requiredPackage in @("Avalonia.Desktop", "Avalonia.Themes.Fluent", "Avalonia.Fonts.Inter")) {
    if ($requiredPackage -notin $demoAvaloniaPackages) { throw "OcctDemo.Avalonia must reference $requiredPackage." }
}

$demoSourceText = Get-TrackedSourceText @("src/OcctDemo.Common", "src/OcctDemo.Avalonia")
foreach ($forbiddenText in @("System.Windows.Forms", "user32.dll", "MessageBoxW", "System.Media.SystemSounds")) {
    if ($demoSourceText.Contains($forbiddenText, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Cross-platform Avalonia demo source must not contain Windows-only dependency: $forbiddenText"
    }
}

foreach ($requiredPath in @("run.ps1", "run.sh", "publish.ps1", "publish.sh")) {
    if (-not (Test-TrackedPath $requiredPath)) { throw "Avalonia demo workflow must track: $requiredPath" }
}

foreach ($forbiddenPath in @(
    "src/OcctNet.WinForms",
    "src/OcctNet.Wpf",
    "src/OcctDemo.WinForms",
    "src/OcctDemo.Wpf",
    "tests/OcctNet.X11Smoke",
    "dist",
    "sync.ps1",
    "sync-dist.ps1"
)) {
    if (Test-TrackedPath $forbiddenPath) { throw "Avalonia branch must not track: $forbiddenPath" }
}

foreach ($legacyProject in @("src/CadCommon", "src/CadWinForms", "src/CadWpf", "src/CadAvalonia")) {
    if (Test-TrackedPath $legacyProject) { throw "Legacy application project must not be tracked: $legacyProject" }
}

$managedText = Get-TrackedSourceText @("src/OcctNet")
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

Write-Host "[architecture] Cross-platform OcctNet + OcctNet.Avalonia and native Avalonia demo boundaries validated for Windows/Linux." -ForegroundColor Green
