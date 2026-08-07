$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "src/OcctNative/OcctObjectIdentity.cpp",
    "src/OcctNative/OcctObjectUpdate.cpp",
    "src/OcctNative/OcctObjectInteraction.cpp",
    "src/OcctNative/OcctObjectTransform.cpp",
    "src/OcctNative/OcctSelectionState.cpp",
    "src/OcctNet/OcctEngine.ObjectIdentity.cs",
    "src/OcctNet/OcctEngine.ObjectUpdate.cs",
    "src/OcctNet/OcctEngine.ObjectInteraction.cs",
    "src/OcctNet/OcctEngine.ObjectTransform.cs",
    "src/OcctNet/OcctEngine.SelectionState.cs"
)
foreach ($path in $requiredFiles) {
    if (-not (Test-Path $path)) { throw "Missing object integration file: $path" }
}

$nativeHeader = Get-Content "src/OcctNative/OcctNative.h" -Raw
$modelingHeader = Get-Content "src/OcctNative/OcctModeling.h" -Raw
$managed = Get-ChildItem "src/OcctNet" -Filter "OcctEngine.*.cs" | Get-Content -Raw | Out-String
$requiredNative = @(
    "occt_set_object_application_tag",
    "occt_update_object_shape_from_model",
    "occt_set_selected_objects_ex",
    "occt_set_object_selectable",
    "occt_set_object_transform",
    "occt_get_object_transform",
    "occt_reset_object_transform",
    "occt_set_view_cube_language"
)
foreach ($name in $requiredNative) {
    $source = if ($name -eq "occt_update_object_shape_from_model") { $modelingHeader } else { $nativeHeader }
    if ($source -notmatch [regex]::Escape($name)) { throw "Missing native declaration: $name" }
}

$requiredManaged = @(
    "SetApplicationTag",
    "TryGetObjectByApplicationTag",
    "UpdateShape",
    "SetSelection",
    "SetSelectable",
    "SetLocalTransformation",
    "GetLocalTransformation",
    "HasLocalTransformation",
    "ResetLocalTransformation",
    "SetViewCubeLanguage"
)
foreach ($name in $requiredManaged) {
    if ($managed -notmatch [regex]::Escape($name)) { throw "Missing managed API: $name" }
}

$winForms = Get-Content "src/OcctNet.WinForms/OcctViewportControl.cs" -Raw
$wpf = Get-Content "src/OcctNet.Wpf/OcctWpfViewport.cs" -Raw
if ($winForms -notmatch "EnableDefaultInteraction" -or $wpf -notmatch "EnableDefaultInteraction") {
    throw "EnableDefaultInteraction is not exposed by both UI hosts."
}

$coverage = Get-Content "docs/API_COVERAGE.md" -Raw
$requiredCoverage = @(
    'Native exports: `339`',
    'Managed P/Invoke declarations: `339`',
    'Public .NET types: `80`',
    'occt_set_object_application_tag',
    'occt_update_object_shape_from_model',
    'OcctObjectTransformUpdate'
)
foreach ($token in $requiredCoverage) {
    if (-not $coverage.Contains($token)) { throw "API inventory is missing: $token" }
}

Write-Host "Object integration API contract passed." -ForegroundColor Green
