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

function Assert-SourceDoesNotContain {
    param(
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][string[]]$Forbidden
    )

    $text = Read-SourceText $RelativePath
    foreach ($value in $Forbidden) {
        if ($text.Contains($value)) {
            throw "$RelativePath must not contain retired ABI symbol or implementation: $value"
        }
    }
}

function Assert-SourceContains {
    param(
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][string[]]$Required
    )

    $text = Read-SourceText $RelativePath
    foreach ($value in $Required) {
        if (-not $text.Contains($value)) {
            throw "$RelativePath is missing current ABI entry point: $value"
        }
    }
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
    "src/OcctNet/OcctEngine.AnnotationShapes.cs",
    "src/OcctNet/NativeMethods.View.cs",
    "src/OcctNet/ObjectBatchNativeMethods.cs",
    "src/OcctNative/OcctRenderSurface.cpp",
    "src/OcctNative/OcctRenderSurface.h",
    "src/OcctNative/OcctVectorAnnotations.cpp",
    "src/OcctNative/OcctView.cpp",
    "src/OcctNative/OcctObjectIdentity.cpp",
    "src/OcctNative/OcctObjectInteraction.cpp"
)) {
    if (Test-TrackedPath $legacyFile) {
        throw "Retired compatibility source must not be reintroduced: $legacyFile"
    }
}

# Currentized domains are ABI5-only: native implementation owns current symbols and must not
# reintroduce retired ABI4 entry points.
Assert-SourceContains "src/OcctNative/geometry/OcctPoints.cpp" @(
    "occt_engine_point_create(",
    "occt_engine_point_update(",
    "occt_engine_point_pixmap_create(",
    "occt_engine_point_pixmap_update("
)
Assert-SourceContains "src/OcctNative/presentation/OcctAppearance.cpp" @(
    "occt_engine_scene_lighting_set(",
    "occt_engine_scene_lighting_reset(",
    "occt_engine_highlight_colors_set("
)
Assert-SourceContains "src/OcctNative/presentation/OcctPresentation.cpp" @(
    "occt_engine_presentation_state_update(",
    "occt_engine_presentation_state_get("
)
Assert-SourceContains "src/OcctNative/OcctObjectTransform.cpp" @(
    "occt_engine_object_transform_set(",
    "occt_engine_object_transform_get(",
    "occt_engine_object_transform_reset(",
    "occt_engine_object_transforms_set("
)
Assert-SourceContains "src/OcctNative/OcctObjectBatch.cpp" @(
    "occt_engine_objects_update(",
    "occt_engine_object_state_get(",
    "occt_engine_objects_presentation_action("
)
Assert-SourceContains "src/OcctNative/OcctObjectUpdate.cpp" @(
    "occt_engine_object_shape_update_from_model("
)
Assert-SourceContains "src/OcctNative/selection/OcctManipulator.cpp" @(
    "occt_engine_manipulator_create(",
    "occt_engine_manipulator_attach(",
    "occt_engine_manipulator_update(",
    "occt_engine_manipulator_transform("
)
Assert-SourceContains "src/OcctNative/viewer/OcctViewerUpdate.cpp" @(
    "occt_engine_update_begin(",
    "occt_engine_update_end(",
    "occt_engine_update_state_get("
)

Assert-SourceDoesNotContain "src/OcctNative/OcctObjectTransform.cpp" @(
    "occt_set_object_transform(",
    "occt_get_object_transform(",
    "occt_reset_object_transform(",
    "occt_set_object_transforms("
)
Assert-SourceDoesNotContain "src/OcctNative/OcctObjectBatch.cpp" @(
    "occt_set_objects_color(",
    "occt_set_objects_transparency(",
    "occt_set_objects_visible(",
    "occt_set_objects_display_mode(",
    "occt_set_objects_line_width(",
    "occt_set_objects_material(",
    "occt_redisplay_objects(",
    "occt_select_objects(",
    "occt_object_is_visible(",
    "occt_object_is_selected("
)
Assert-SourceDoesNotContain "src/OcctNative/OcctObjectUpdate.cpp" @(
    "occt_update_object_shape_from_model("
)
Assert-SourceDoesNotContain "src/OcctNative/selection/OcctManipulator.cpp" @(
    "occt_add_manipulator(",
    "occt_attach_manipulator(",
    "occt_detach_manipulator(",
    "occt_set_manipulator_",
    "occt_get_manipulator_",
    "occt_start_manipulator_transform(",
    "occt_update_manipulator_transform(",
    "occt_stop_manipulator_transform(",
    "occt_deactivate_manipulator_mode("
)

# Managed bindings for completed domains must use source-generated LibraryImport only.
foreach ($currentInterop in @(
    "src/OcctNet/ObjectNativeMethods.cs",
    "src/OcctNet/ObjectTransformNativeMethods.cs",
    "src/OcctNet/ViewerModelInteropNativeMethods.cs",
    "src/OcctNet/BatchNativeMethods.cs",
    "src/OcctNet/ManipulatorNativeMethods.cs",
    "src/OcctNet/ViewNativeMethods.cs"
)) {
    $text = Read-SourceText $currentInterop
    if ($text.Contains("[DllImport(")) {
        throw "$currentInterop must use LibraryImport only."
    }
}

foreach ($legacyManagedCall in @(
    "NativeMethods.occt_make_text_shape(",
    "NativeMethods.occt_make_length_annotation_shape(",
    "NativeMethods.occt_make_angle_annotation_shape(",
    "NativeMethods.occt_make_radius_annotation_shape(",
    "NativeMethods.occt_make_diameter_annotation_shape(",
    "NativeMethods.occt_add_point(",
    "NativeMethods.occt_set_point_position(",
    "NativeMethods.occt_set_point_style(",
    "NativeMethods.occt_add_point_pixmap(",
    "NativeMethods.occt_set_point_pixmap_style(",
    "NativeMethods.occt_update_object_shape_from_model(",
    "NativeMethods.occt_set_object_transform(",
    "NativeMethods.occt_get_object_transform(",
    "NativeMethods.occt_reset_object_transform(",
    "NativeMethods.occt_set_objects_color(",
    "NativeMethods.occt_set_objects_transparency(",
    "NativeMethods.occt_set_objects_visible(",
    "NativeMethods.occt_set_objects_display_mode(",
    "NativeMethods.occt_set_objects_line_width(",
    "NativeMethods.occt_set_objects_material(",
    "NativeMethods.occt_redisplay_objects(",
    "NativeMethods.occt_select_objects(",
    "NativeMethods.occt_object_is_visible(",
    "NativeMethods.occt_object_is_selected("
)) {
    if ($managedText.Contains($legacyManagedCall)) {
        throw "Managed production API must not call retired export: $legacyManagedCall"
    }
}

$layoutName = if ($isDemoBranchLayout) { "demo" } else { "main" }
Write-Host "[architecture] Core/UI boundaries, $layoutName branch layout, and ABI5-only currentized domains validated." -ForegroundColor Green
