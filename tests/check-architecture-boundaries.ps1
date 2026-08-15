param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Project {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) { throw "Project file was not found: $RelativePath" }
    return [xml](Get-Content $path -Raw -Encoding UTF8)
}

function Get-ProjectReferences {
    param([Parameter(Mandatory = $true)][xml]$Project)
    return @($Project.SelectNodes('/Project/ItemGroup/ProjectReference') | ForEach-Object {
        [string]$_.GetAttribute('Include')
    } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-PackageReferences {
    param([Parameter(Mandatory = $true)][xml]$Project)
    return @($Project.SelectNodes('/Project/ItemGroup/PackageReference') | ForEach-Object {
        [string]$_.GetAttribute('Include')
    } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-ProjectProperty {
    param([Parameter(Mandatory = $true)][xml]$Project, [Parameter(Mandatory = $true)][string]$Name)
    $node = $Project.SelectSingleNode("/Project/PropertyGroup/$Name[normalize-space(.) != '']")
    if ($null -eq $node) { return $null }
    return [string]$node.InnerText
}

function Assert-Reference {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$References,
        [Parameter(Mandatory = $true)][string]$Expected,
        [Parameter(Mandatory = $true)][string]$ProjectName)
    $normalizedExpected = $Expected.Replace('/', '\')
    $matches = @($References | Where-Object { $_.Replace('/', '\') -eq $normalizedExpected })
    if ($matches.Count -ne 1) { throw "$ProjectName must reference exactly once: $Expected" }
}

function Assert-NoUiSiblingReferences {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$References,
        [Parameter(Mandatory = $true)][string]$ProjectName)
    foreach ($reference in $References) {
        foreach ($sibling in @("OcctNet.WinForms", "OcctNet.Wpf", "OcctNet.Avalonia")) {
            if ($reference -match "(?i)\\$([regex]::Escape($sibling))\\") {
                throw "$ProjectName must not depend on sibling UI host project $sibling."
            }
        }
    }
}

function Test-TrackedPath {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $normalized = $RelativePath.Replace('\', '/')
    $tracked = @(& git -C $RepositoryRoot ls-files -- $normalized "$normalized/**" 2>$null)
    if ($LASTEXITCODE -ne 0) { throw "Unable to inspect tracked repository paths with git ls-files." }
    return $tracked.Count -gt 0
}

$core = Read-Project "src/OcctNet/OcctNet.csproj"
$winForms = Read-Project "src/OcctNet.WinForms/OcctNet.WinForms.csproj"
$wpf = Read-Project "src/OcctNet.Wpf/OcctNet.Wpf.csproj"
$avalonia = Read-Project "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj"

$coreProjectReferences = @(Get-ProjectReferences $core)
$corePackageReferences = @(Get-PackageReferences $core)
if ($coreProjectReferences.Count -ne 0) {
    throw "OcctNet core must not depend on UI host or packaging projects."
}
foreach ($uiDependency in @("Avalonia", "PresentationFramework", "System.Windows.Forms")) {
    if ($uiDependency -in $corePackageReferences) {
        throw "OcctNet core must remain UI-framework independent: $uiDependency"
    }
}

$winFormsReferences = @(Get-ProjectReferences $winForms)
$wpfReferences = @(Get-ProjectReferences $wpf)
$avaloniaReferences = @(Get-ProjectReferences $avalonia)
Assert-Reference $winFormsReferences "..\OcctNet\OcctNet.csproj" "OcctNet.WinForms"
Assert-Reference $wpfReferences "..\OcctNet\OcctNet.csproj" "OcctNet.Wpf"
Assert-Reference $avaloniaReferences "..\OcctNet\OcctNet.csproj" "OcctNet.Avalonia"
Assert-NoUiSiblingReferences $winFormsReferences "OcctNet.WinForms"
Assert-NoUiSiblingReferences $wpfReferences "OcctNet.Wpf"
Assert-NoUiSiblingReferences $avaloniaReferences "OcctNet.Avalonia"

if ((Get-ProjectProperty $winForms "UseWindowsForms") -ne "true") {
    throw "OcctNet.WinForms must enable Windows Forms."
}
if ((Get-ProjectProperty $wpf "UseWPF") -ne "true") {
    throw "OcctNet.Wpf must enable WPF."
}
if (-not [string]::IsNullOrWhiteSpace((Get-ProjectProperty $wpf "UseWindowsForms"))) {
    throw "OcctNet.Wpf must remain independent from Windows Forms."
}
if ("Avalonia" -notin @(Get-PackageReferences $avalonia)) {
    throw "OcctNet.Avalonia must reference the Avalonia package."
}

# Reusable core must not absorb application architecture.
$managedRoot = Join-Path $RepositoryRoot "src\OcctNet"
$managedFiles = @(Get-ChildItem $managedRoot -Filter '*.cs' -File -Recurse)
$managedText = ($managedFiles | ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"
foreach ($forbidden in @("DocumentManager", "CommandBus", "CommandRegistry", "ToolManager")) {
    if ($managedText -match "\b$([regex]::Escape($forbidden))\b") {
        throw "Application-layer type must not enter OcctNet core: $forbidden"
    }
}

# ABI5 production interop uses source-generated LibraryImport only.
$dllImportFiles = @($managedFiles | Where-Object {
    [System.IO.File]::ReadAllText($_.FullName).Contains("[DllImport(")
})
if ($dllImportFiles.Count -gt 0) {
    $relative = @($dllImportFiles | ForEach-Object { $_.FullName.Substring($RepositoryRoot.Length).TrimStart('\') })
    throw "DllImport is forbidden in OcctNet ABI5 production code: $($relative -join ', ')"
}

# Compatibility infrastructure is not part of Bridge 3.
foreach ($retired in @(
    "tests/contracts/abi4-exports.txt",
    "tests/check-abi-compatibility.ps1",
    "tests/compatibility",
    "src/OcctNet/OcctObject.Legacy.cs",
    "src/OcctNet/OcctGeometryExtensions.Compatibility.cs",
    "src/OcctNet/OcctEngine.ApiAliases.cs",
    "src/OcctNet/NativeMethods.Modeling.cs",
    "src/OcctNative/OcctRenderSurface.cpp",
    "src/OcctNative/OcctRenderSurface.h",
    "src/OcctNative/OcctVectorAnnotations.cpp",
    "src/OcctNative/OcctViewerInteraction.cpp",
    "src/OcctNative/OcctViewerInteraction.h",
    "src/OcctNative/OcctViewerInteractionExtensions.cpp",
    "src/OcctNative/OcctViewerInteractionExtensions.h",
    "src/OcctNative/OcctViewportExtensions.cpp",
    "src/OcctNative/OcctModelingExtensions.cpp",
    "src/OcctNative/OcctModelingExtensions.h",
    "src/OcctNative/OcctEngineShapes.cpp",
    "src/OcctNative/OcctFeatures.cpp",
    "src/OcctNative/geometry/OcctQueries.cpp",
    "src/OcctNative/geometry/OcctGeometry.cpp",
    "src/OcctNative/geometry/OcctPlanarGeometry.cpp",
    "src/OcctNative/geometry/OcctModelingGeometry.Curves.cpp",
    "src/OcctNative/geometry/OcctModelingGeometry.Planar.cpp",
    "src/OcctNative/geometry/OcctModelingGeometry.Primitives.cpp",
    "src/OcctNative/geometry/OcctModelingGeometry.Assembly.cpp",
    "src/OcctNative/geometry/OcctModelingGeometry.Transform.cpp"
)) {
    if (Test-TrackedPath $retired) { throw "Retired source or compatibility path must not be reintroduced: $retired" }
}

# Native module filenames must express ownership. Extension/Helper/Utils/Misc modules are
# intentionally forbidden at the native ABI boundary; internal .hxx names may use specific
# implementation nouns but not generic dumping-ground names.
$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$nativeFiles = @(Get-ChildItem $nativeRoot -File -Recurse)
$badNativeNames = @($nativeFiles | Where-Object {
    $_.Name -match '(?i)(Extension|Extensions|Helper|Helpers|Utils|Utilities|Misc)\.(cpp|cxx|h|hpp|hxx)$'
})
if ($badNativeNames.Count -gt 0) {
    $relative = @($badNativeNames | ForEach-Object { $_.FullName.Substring($RepositoryRoot.Length).TrimStart('\') })
    throw "Native modules must use semantic domain names instead of generic utility names: $($relative -join ', ')"
}

# Old exported ABI naming is forbidden. Internal helper names are not checked here;
# check-api-surface.ps1 owns the exact public declaration/definition/import set.
$nativeText = ($nativeFiles | Where-Object { $_.Extension -in @('.cpp', '.cxx', '.h', '.hpp') } |
    ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"
$legacyExportPattern = '(?m)^\s*(?:OCCTBRIDGE_API\s+)?(?:const\s+char\s*\*|char\s*\*|void|int|long|std::int64_t|OcctObjectId|OcctHandle)\s+(occt_(?!engine_|model_|shape_|mesh_|algorithm_|version\b|bridge_version\b|bridge_build_info\b|bridge_current_abi_version\b)[a-z0-9_]+)\s*\('
$legacyExports = @([regex]::Matches($nativeText, $legacyExportPattern) | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
if ($legacyExports.Count -gt 0) {
    throw "Legacy native ABI entry points remain: $($legacyExports -join ', ')"
}

# Demo projects are not allowed on main/main-dev. On demo branches all three must exist as a set.
$demoProjects = @("src/OcctDemo.Common", "src/OcctDemo.WinForms", "src/OcctDemo.Wpf")
$trackedDemoProjects = @($demoProjects | Where-Object { Test-TrackedPath $_ })
if ($trackedDemoProjects.Count -ne 0 -and $trackedDemoProjects.Count -ne $demoProjects.Count) {
    throw "Demo projects must be either fully absent on main or fully present on demo."
}
if (Test-TrackedPath "src/OcctDemo.Avalonia") {
    throw "Avalonia demo must live on the avalonia branch, not main/demo."
}

foreach ($legacyProject in @("src/CadCommon", "src/CadWinForms", "src/CadWpf", "src/CadAvalonia")) {
    if (Test-TrackedPath $legacyProject) { throw "Legacy application project must not be tracked: $legacyProject" }
}

if ($trackedDemoProjects.Count -eq $demoProjects.Count) {
    $demoCommon = Read-Project "src/OcctDemo.Common/OcctDemo.Common.csproj"
    $demoWinForms = Read-Project "src/OcctDemo.WinForms/OcctDemo.WinForms.csproj"
    $demoWpf = Read-Project "src/OcctDemo.Wpf/OcctDemo.Wpf.csproj"
    Assert-Reference @(Get-ProjectReferences $demoCommon) "..\OcctNet\OcctNet.csproj" "OcctDemo.Common"
    Assert-Reference @(Get-ProjectReferences $demoWinForms) "..\OcctDemo.Common\OcctDemo.Common.csproj" "OcctDemo.WinForms"
    Assert-Reference @(Get-ProjectReferences $demoWinForms) "..\OcctNet.WinForms\OcctNet.WinForms.csproj" "OcctDemo.WinForms"
    Assert-Reference @(Get-ProjectReferences $demoWpf) "..\OcctDemo.Common\OcctDemo.Common.csproj" "OcctDemo.Wpf"
    Assert-Reference @(Get-ProjectReferences $demoWpf) "..\OcctNet.Wpf\OcctNet.Wpf.csproj" "OcctDemo.Wpf"
}

Write-Host "[architecture] ABI5-only SDK boundaries validated." -ForegroundColor Green
