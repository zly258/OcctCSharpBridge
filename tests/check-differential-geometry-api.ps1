param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contracts = [ordered]@{
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
        throw "Differential geometry API file was not found: $($contract.Key)"
    }
    $text = [System.IO.File]::ReadAllText($path)
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Differential geometry token is missing from $($contract.Key): $token"
        }
    }
}

Write-Host "[differential-geometry] Curve and surface derivatives, periodicity and curvature contracts validated." -ForegroundColor Green
