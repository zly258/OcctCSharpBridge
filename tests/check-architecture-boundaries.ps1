param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Project {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Project file was not found: $RelativePath"
    }

    return [xml](Get-Content $path -Raw -Encoding UTF8)
}

function Read-SourceText {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Source file was not found: $RelativePath"
    }
    return [System.IO.File]::ReadAllText($path)
}

function Get-ProjectReferences {
    param([Parameter(Mandatory = $true)][xml]$Project)

    $nodes = @($Project.SelectNodes('/Project/ItemGroup/ProjectReference'))
    return @($nodes | ForEach-Object {
        [string]$_.GetAttribute('Include')
    } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-PackageReferences {
    param([Parameter(Mandatory = $true)][xml]$Project)

    $nodes = @($Project.SelectNodes('/Project/ItemGroup/PackageReference'))
    return @($nodes | ForEach-Object {
        [string]$_.GetAttribute('Include')
    } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-ProjectProperty {
    param(
        [Parameter(Mandatory = $true)][xml]$Project,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $node = $Project.SelectSingleNode("/Project/PropertyGroup/$Name[normalize-space(.) != '']")
    if ($null -eq $node) { return $null }
    return [string]$node.InnerText
}

function Assert-Reference {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$References,
        [Parameter(Mandatory = $true)][string]$Expected,
        [Parameter(Mandatory = $true)][string]$ProjectName
    )

    $normalizedExpected = $Expected.Replace('/', '\')
    $matches = @($References | Where-Object { $_.Replace('/', '\') -eq $normalizedExpected })
    if ($matches.Count -ne 1) {
        throw "$ProjectName must reference exactly once: $Expected"
    }
}

function Assert-NoUiSiblingReferences {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$References,
        [Parameter(Mandatory = $true)][string]$ProjectName
    )

    $siblings = @("OcctNet.WinForms", "OcctNet.Wpf", "OcctNet.Avalonia")
    foreach ($reference in $References) {
        foreach ($sibling in $siblings) {
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
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to inspect tracked repository paths with git ls-files."
    }
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

$demoProjects = @(
    "src/OcctDemo.Common",
    "src/OcctDemo.WinForms",
    "src/OcctDemo.Wpf"
)
$trackedDemoProjects = @($demoProjects | Where-Object { Test-TrackedPath $_ })
if ($trackedDemoProjects.Count -ne 0 -and $trackedDemoProjects.Count -ne $demoProjects.Count) {
    throw "Demo projects must be either fully absent on main or fully present on demo."
}
$isDemoBranchLayout = $trackedDemoProjects.Count -eq $demoProjects.Count

if (Test-TrackedPath "src/OcctDemo.Avalonia") {
    throw "Avalonia demo must live on the avalonia branch, not demo."
}

foreach ($legacyProject in @(
    "src/CadCommon",
    "src/CadWinForms",
    "src/CadWpf",
    "src/CadAvalonia"
)) {
    if (Test-TrackedPath $legacyProject) {
        throw "Legacy application project must not be tracked: $legacyProject"
    }
}

if ($isDemoBranchLayout) {
    $demoCommon = Read-Project "src/OcctDemo.Common/OcctDemo.Common.csproj"
    $demoWinForms = Read-Project "src/OcctDemo.WinForms/OcctDemo.WinForms.csproj"
    $demoWpf = Read-Project "src/OcctDemo.Wpf/OcctDemo.Wpf.csproj"

    Assert-Reference @(Get-ProjectReferences $demoCommon) "..\OcctNet\OcctNet.csproj" "OcctDemo.Common"
    Assert-Reference @(Get-ProjectReferences $demoWinForms) "..\OcctDemo.Common\OcctDemo.Common.csproj" "OcctDemo.WinForms"
    Assert-Reference @(Get-ProjectReferences $demoWinForms) "..\OcctNet.WinForms\OcctNet.WinForms.csproj" "OcctDemo.WinForms"
    Assert-Reference @(Get-ProjectReferences $demoWpf) "..\OcctDemo.Common\OcctDemo.Common.csproj" "OcctDemo.Wpf"
    Assert-Reference @(Get-ProjectReferences $demoWpf) "..\OcctNet.Wpf\OcctNet.Wpf.csproj" "OcctDemo.Wpf"
}

$managedRoot = Join-Path $RepositoryRoot "src\OcctNet"
$managedText = (Get-ChildItem $managedRoot -Filter '*.cs' -File -Recurse |
    ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"

foreach ($forbidden in @(
    "DocumentManager",
    "CommandBus",
    "CommandRegistry",
    "ToolManager"
)) {
    if ($managedText -match "\b$([regex]::Escape($forbidden))\b") {
        throw "Application-layer type must not enter OcctNet core: $forbidden"
    }
}

foreach ($legacyFile in @(
    "src/OcctNet/OcctObject.Legacy.cs",
    "src/OcctNet/OcctGeometryExtensions.Compatibility.cs",
    "src/OcctNet/OcctEngine.ApiAliases.cs",
    "src/OcctNet/NativeMethods.Modeling.cs",
    "src/OcctNet/OcctEngine.AnnotationShapes.cs"
)) {
    if (Test-TrackedPath $legacyFile) {
        throw "Legacy/compatibility source must not be reintroduced: $legacyFile"
    }
}

# ABI 4 stays as a compatibility shell. Current ABI owns the implementation path.
$engineSource = Read-SourceText "src/OcctNative/core/OcctEngine.cpp"
if (-not $engineSource.Contains("return reinterpret_cast<OcctHandle>(occt_engine_create());")) {
    throw "Legacy occt_create must delegate to occt_engine_create."
}
if (-not $engineSource.Contains("occt_engine_destroy(reinterpret_cast<OcctEngineHandle>(handle));")) {
    throw "Legacy occt_destroy must delegate to occt_engine_destroy."
}

$surfaceSource = Read-SourceText "src/OcctNative/platform/OcctNativeSurface.cpp"
if (-not $surfaceSource.Contains("return occt_engine_initialize_surface(")) {
    throw "Legacy native-surface initialization must delegate to the current surface API."
}
if ($surfaceSource.Contains("return occt_initialize_surface(reinterpret_cast<OcctHandle>(handle)")) {
    throw "Current native-surface API must not delegate back into the legacy ABI."
}

# BRep annotation construction has one headless implementation. The frozen viewer ABI may
# adapt registry objects into it, but must never grow its own font, text, arrow or dimension geometry.
$vectorAnnotationSource = Read-SourceText "src/OcctNative/OcctVectorAnnotations.cpp"
foreach ($sharedBuilder in @(
    "buildBRepText(",
    "buildLengthAnnotation(",
    "buildAngleAnnotation(",
    "buildRadiusAnnotation(",
    "buildDiameterAnnotation("
)) {
    if (-not $vectorAnnotationSource.Contains($sharedBuilder)) {
        throw "Legacy vector annotation adapter must delegate to shared headless builder: $sharedBuilder"
    }
}
foreach ($forbiddenImplementation in @(
    "StdPrs_BRepFont",
    "StdPrs_BRepTextBuilder",
    "BRepBuilderAPI_MakePolygon",
    "BRepBuilderAPI_MakeFace"
)) {
    if ($vectorAnnotationSource.Contains($forbiddenImplementation)) {
        throw "Legacy vector annotation adapter must not own geometry implementation: $forbiddenImplementation"
    }
}

$viewerAnnotationSource = Read-SourceText "src/OcctNative/OcctAnnotations.cpp"
foreach ($currentEntryPoint in @(
    "occt_engine_text_create(",
    "occt_engine_text_update(",
    "occt_engine_dimension_create(",
    "occt_engine_dimension_update("
)) {
    if (-not $viewerAnnotationSource.Contains($currentEntryPoint)) {
        throw "Viewer annotation ABI 4 shell must route through current entry point: $currentEntryPoint"
    }
}

# Production managed APIs must not call frozen BRep annotation exports. They are retained only
# as ABI declarations until the dedicated compatibility assembly owns every ABI 4 declaration.
foreach ($legacyManagedCall in @(
    "NativeMethods.occt_make_text_shape(",
    "NativeMethods.occt_make_length_annotation_shape(",
    "NativeMethods.occt_make_angle_annotation_shape(",
    "NativeMethods.occt_make_radius_annotation_shape(",
    "NativeMethods.occt_make_diameter_annotation_shape("
)) {
    if ($managedText.Contains($legacyManagedCall)) {
        throw "Managed production API must not call frozen BRep annotation export: $legacyManagedCall"
    }
}

$layoutName = if ($isDemoBranchLayout) { "demo" } else { "main" }
Write-Host "[architecture] Core/UI dependency direction, WinForms/WPF/Avalonia hosts, $layoutName branch layout, current-over-legacy adapter direction, shared annotation implementation, and no-compatibility boundary validated." -ForegroundColor Green
