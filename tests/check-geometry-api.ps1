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
    "src/OcctNative/OcctModelingDifferentialGeometry.cpp" = @(
        "BRepLProp_CLProps",
        "BRepLProp_SLProps",
        "occt_model_edge_parameter_range",
        "occt_model_edge_differential",
        "occt_model_edge_curvature",
        "occt_model_face_periodicity",
        "occt_model_face_differential",
        "occt_model_face_curvature"
    )
    "src/OcctNet/ModelNativeMethods.DifferentialGeometry.cs" = @(
        "occt_model_edge_parameter_range",
        "occt_model_face_curvature",
        "CallingConvention.Cdecl",
        "ExactSpelling = true"
    )
    "src/OcctNet/OcctModelingSession.DifferentialGeometry.cs" = @(
        "GetEdgeParameterRange",
        "EvaluateEdgeAtParameter",
        "GetEdgeCurvature",
        "GetFacePeriodicity",
        "EvaluateFaceDifferential",
        "GetFaceCurvature"
    )
    "src/OcctNet/OcctDifferentialGeometryTypes.cs" = @(
        "OcctModelParameterRange",
        "OcctModelCurveDifferential",
        "OcctModelCurveCurvature",
        "OcctModelSurfacePeriodicity",
        "OcctModelSurfaceDifferential",
        "OcctModelSurfaceCurvature",
        "StructLayout(LayoutKind.Sequential)"
    )
}

foreach ($contract in $contracts.GetEnumerator()) {
    $path = Join-Path $RepositoryRoot $contract.Key
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Geometry API file was not found: $($contract.Key)"
    }

    $text = [System.IO.File]::ReadAllText($path)
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Geometry API token is missing from $($contract.Key): $token"
        }
    }
}

Write-Host "[geometry] Analytic and differential geometry contracts validated." -ForegroundColor Green
