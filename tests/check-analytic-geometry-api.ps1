param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contracts = [ordered]@{
    "src/OcctNative/OcctModelingAnalyticGeometry.cpp" = @(
        "occt_model_edge_line_geometry",
        "occt_model_edge_circle_geometry",
        "occt_model_edge_ellipse_geometry",
        "occt_model_face_plane_geometry",
        "occt_model_face_cylinder_geometry",
        "occt_model_face_cone_geometry",
        "occt_model_face_sphere_geometry",
        "occt_model_face_torus_geometry"
    )
    "src/OcctNet/ModelNativeMethods.AnalyticGeometry.cs" = @(
        "occt_model_edge_line_geometry",
        "occt_model_face_torus_geometry",
        "CallingConvention.Cdecl"
    )
    "src/OcctNet/OcctModelingSession.AnalyticGeometry.cs" = @(
        "GetLineGeometry",
        "GetCircleGeometry",
        "GetEllipseGeometry",
        "GetPlaneGeometry",
        "GetCylinderGeometry",
        "GetConeGeometry",
        "GetSphereGeometry",
        "GetTorusGeometry"
    )
    "src/OcctNet/OcctAnalyticGeometryTypes.cs" = @(
        "OcctLineGeometry",
        "OcctCircleGeometry",
        "OcctEllipseGeometry",
        "OcctPlaneGeometry",
        "OcctCylinderGeometry",
        "OcctConeGeometry",
        "OcctSphereGeometry",
        "OcctTorusGeometry",
        "StructLayout(LayoutKind.Sequential)"
    )
}

foreach ($contract in $contracts.GetEnumerator()) {
    $path = Join-Path $RepositoryRoot $contract.Key
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Analytic geometry API file was not found: $($contract.Key)"
    }

    $text = [System.IO.File]::ReadAllText($path)
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Analytic geometry API token is missing from $($contract.Key): $token"
        }
    }
}

Write-Host "[analytic-geometry] Line, circle, ellipse, plane, cylinder, cone, sphere and torus query contracts validated." -ForegroundColor Green
