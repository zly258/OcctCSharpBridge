param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot "ContractTestHelpers.psm1") -Force

$required = [ordered]@{
    "src/OcctNative/OcctViewportExtensions.cpp" = @(
        "occt_fit_objects", "occt_set_zup_view", "occt_screen_to_ray", "occt_zoom_at_point",
        "occt_select_all_visible", "occt_invert_selection", "occt_hide_selected",
        "occt_set_msaa_samples", "occt_set_rendering_method", "occt_set_face_boundaries_visible"
    )
    "src/OcctNative/OcctViewportState.cpp" = @(
        "occt_get_viewport_state", "occt_reset_view", "occt_reset_view_orientation",
        "occt_reset_view_mapping", "occt_fit_selected", "occt_get_scene_gravity_point"
    )
    "src/OcctNet/OcctEngine.Viewport.cs" = @(
        "Fit(IEnumerable<OcctShape>", "SetZUpView", "ScreenToRay", "ZoomAtPoint",
        "SelectAllVisible", "InvertSelection", "HideSelected", "SetMsaaSamples",
        "SetRenderingMethod", "SetFaceBoundariesVisible", "GetViewportState",
        "ResetView", "FitSelected", "GetSceneGravityPoint", "ScreenToPlane"
    )
    "src/OcctNet/OcctRuntime.Probing.cs" = @(
        "portableRuntimeDirectory", "portableOcctRoot"
    )
}

Assert-ContractMap -RepositoryRoot $RepositoryRoot -Contracts $required -ContractName "Viewport API"

$runtimeEnvironment = Get-ContractText -RepositoryRoot $RepositoryRoot -RelativePath "src/OcctNet/OcctRuntime.Environment.cs"
foreach ($forbidden in @("CSF_TObjMessage", "CSF_XCAFDefaults", "CSF_XmlOcafResource")) {
    Assert-TextNotContains -Text $runtimeEnvironment -Token $forbidden -Message "OCAF/XDE runtime configuration remains: $forbidden"
}

Write-Host "[viewport] Extended view, selection, rendering, and split portable-runtime contracts validated." -ForegroundColor Green
