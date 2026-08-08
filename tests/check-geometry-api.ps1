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
        "occt_model_edge_line_geometry", "occt_model_face_torus_geometry", "CallingConvention.Cdecl"
    )
    "src/OcctNet/OcctModelingSession.AnalyticGeometry.cs" = @(
        "GetLineGeometry", "GetCircleGeometry", "GetEllipseGeometry", "GetPlaneGeometry",
        "GetCylinderGeometry", "GetConeGeometry", "GetSphereGeometry", "GetTorusGeometry"
    )
    "src/OcctNet/OcctAnalyticGeometryTypes.cs" = @(
        "OcctLineGeometry", "OcctCircleGeometry", "OcctEllipseGeometry", "OcctPlaneGeometry",
        "OcctCylinderGeometry", "OcctConeGeometry", "OcctSphereGeometry", "OcctTorusGeometry",
        "StructLayout(LayoutKind.Sequential)"
    )
    "src/OcctNative/OcctModelingDifferentialGeometry.cpp" = @(
        "BRepLProp_CLProps", "BRepLProp_SLProps", "occt_model_edge_parameter_range",
        "occt_model_edge_differential", "occt_model_edge_curvature", "occt_model_face_periodicity",
        "occt_model_face_differential", "occt_model_face_curvature"
    )
    "src/OcctNet/ModelNativeMethods.DifferentialGeometry.cs" = @(
        "occt_model_edge_parameter_range", "occt_model_face_curvature", "CallingConvention.Cdecl", "ExactSpelling = true"
    )
    "src/OcctNet/OcctModelingSession.DifferentialGeometry.cs" = @(
        "GetEdgeParameterRange", "EvaluateEdgeAtParameter", "GetEdgeCurvature",
        "GetFacePeriodicity", "EvaluateFaceDifferential", "GetFaceCurvature"
    )
    "src/OcctNet/OcctDifferentialGeometryTypes.cs" = @(
        "OcctModelParameterRange", "OcctModelCurveDifferential", "OcctModelCurveCurvature",
        "OcctModelSurfacePeriodicity", "OcctModelSurfaceDifferential", "OcctModelSurfaceCurvature",
        "StructLayout(LayoutKind.Sequential)"
    )
    "src/OcctNative/OcctModelingBSpline.cpp" = @(
        "Geom_BSplineCurve", "Geom_BSplineSurface", "occt_model_edge_bspline_info",
        "occt_model_edge_bspline_pole_at", "occt_model_edge_bspline_knot_at",
        "occt_model_face_bspline_info", "occt_model_face_bspline_pole_at",
        "occt_model_face_bspline_u_knot_at", "occt_model_face_bspline_v_knot_at"
    )
    "src/OcctNative/OcctModelingBSpline.h" = @(
        "OcctModelBSplineCurveInfo", "OcctModelBSplineSurfaceInfo",
        "occt_model_edge_bspline_info", "occt_model_face_bspline_info"
    )
    "src/OcctNet/ModelNativeMethods.BSpline.cs" = @(
        "occt_model_edge_bspline_info", "occt_model_edge_bspline_pole_at",
        "occt_model_edge_bspline_knot_at", "occt_model_face_bspline_info",
        "occt_model_face_bspline_pole_at", "occt_model_face_bspline_u_knot_at",
        "occt_model_face_bspline_v_knot_at", "CallingConvention.Cdecl", "ExactSpelling = true"
    )
    "src/OcctNet/OcctModelingSession.BSpline.cs" = @(
        "GetBSplineCurveData", "GetBSplineSurfaceData", "OcctBSplineCurveData",
        "OcctBSplineSurfaceData", "EnsureValidPole", "EnsureValidKnot"
    )
    "src/OcctNet/OcctBSplineTypes.cs" = @(
        "OcctBSplineCurveData", "OcctBSplineSurfaceData", "UPoleCount", "VPoleCount",
        "GetPole", "GetWeight", "UKnots", "VKnots",
        "OcctModelBSplineCurveInfoNative", "OcctModelBSplineSurfaceInfoNative"
    )
    "src/OcctNet/OcctGeometryExtensions.Core.cs" = @(
        "public static partial class OcctGeometryExtensions", "Lerp", "AngleTo", "ProjectOnto",
        "GetVolume", "GetDiagonalLength", "Expanded", "Union", "GetCenter", "IsWithin"
    )
    "src/OcctNet/OcctGeometryExtensions.Transform.cs" = @(
        "public static partial class OcctGeometryExtensions", "IsAffine", "TransformPoint", "Multiply",
        "TryInvert", "ToTransform3d", "ToModelLocation", "CreateTranslationLocation",
        "CreateUniformScaleLocation", "CreateRotationLocation", "CreateRotationTransform"
    )
    "src/OcctNet/OcctModelingSession.Mesh.cs" = @(
        "GetFaceMesh", "GetShapeMesh(", "GetShapeMeshData", "OcctShapeMeshFaceRange", "new OcctShapeMeshData"
    )
    "src/OcctNet/OcctMeshProvenanceTypes.cs" = @(
        "OcctShapeMeshFaceRange", "OcctShapeMeshData", "FaceRanges", "TryGetFaceForNode",
        "GetFaceForNode", "TryGetFaceForTriangle", "GetFaceForTriangle"
    )
    "src/OcctNative/OcctModelingFaceAnalysis.h" = @(
        "OcctModelFaceAnalysis", "occt_model_shape_face_analysis", "maximumTolerance", "uvBounds", "bounds"
    )
    "src/OcctNative/OcctModelingFaceAnalysis.cpp" = @(
        "BRepAdaptor_Surface", "BRepGProp::SurfaceProperties", "BRepTools::UVBounds",
        "occt_model_shape_face_analysis", "TopExp::MapShapes(root, TopAbs_FACE"
    )
    "src/OcctNet/ModelNativeMethods.FaceAnalysis.cs" = @(
        "occt_model_shape_face_analysis", "NativeModelFaceAnalysis", "CallingConvention.Cdecl", "ExactSpelling = true"
    )
    "src/OcctNet/OcctModelingSession.FaceAnalysis.cs" = @(
        "AnalyzeFaces", "OcctFaceAnalysisResult", "NativeModelFaceAnalysis", "Native face analysis count changed"
    )
    "src/OcctNet/OcctFaceAnalysisTypes.cs" = @(
        "OcctFaceAnalysisInfo", "OcctFaceAnalysisResult", "SurfaceTypeCounts", "TotalArea",
        "MaximumTolerance", "GetFacesBySurfaceType", "NativeModelFaceAnalysis"
    )
    "src/OcctNet/OcctModelingSession.ShapeInspection.cs" = @(
        "InspectShape", "AnalyzeEdgeAdjacency", "AnalyzeFaces", "AnalyzeFreeBounds",
        "GenerateMeshStatistics", "GetShapeMeshData"
    )
    "src/OcctNet/OcctShapeInspectionTypes.cs" = @(
        "OcctShapeInspectionOptions", "OcctShapeInspectionReport", "IncludeFreeBounds",
        "GenerateMeshStatistics", "EdgeAdjacency", "FaceAnalysis", "IncludesMeshStatistics"
    )
    "tests/OcctNet.Smoke/ShapeMeshProvenanceSmoke.cs" = @(
        "GetShapeMeshData", "FaceRanges", "GetFaceForNode", "GetFaceForTriangle"
    )
    "tests/OcctNet.Smoke/ShapeInspectionSmoke.cs" = @(
        "AnalyzeFaces", "InspectShape", "SurfaceTypeCounts", "GenerateMeshStatistics", "MeshTriangleCount"
    )
    "docs/MESH_PROVENANCE.md" = @(
        "Shape Mesh Face Provenance", "GetShapeMeshData()", "OcctShapeMeshFaceRange", "TryGetFaceForTriangle"
    )
    "docs/MESH_PROVENANCE.zh-CN.md" = @(
        "Shape Mesh Face 来源追溯", "GetShapeMeshData()", "OcctShapeMeshFaceRange", "TryGetFaceForTriangle"
    )
    "docs/SHAPE_INSPECTION.md" = @(
        "Batch Face Analysis and Shape Inspection", "AnalyzeFaces()", "InspectShape()", "OcctShapeInspectionReport",
        "occt_model_shape_face_analysis"
    )
    "docs/SHAPE_INSPECTION.zh-CN.md" = @(
        "批量 Face 分析与 Shape 检查", "AnalyzeFaces()", "InspectShape()", "OcctShapeInspectionReport",
        "occt_model_shape_face_analysis"
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

$legacyGeometryExtensions = Join-Path $RepositoryRoot "src\OcctNet\OcctGeometryExtensions.cs"
if (Test-Path $legacyGeometryExtensions -PathType Leaf) {
    throw "Monolithic OcctGeometryExtensions.cs must not be reintroduced; keep core and transform helpers in dedicated partial files."
}

Write-Host "[geometry] Analytic, differential, B-Spline, managed geometry modules, mesh provenance, batch face analysis, and shape inspection contracts validated." -ForegroundColor Green
