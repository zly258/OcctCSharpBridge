# Batch Face Analysis and Shape Inspection

OcctCSharpBridge provides two complementary APIs for large STEP/BIM model inspection:

- `AnalyzeFaces()` collects common face geometry/topology metadata in one native batch operation.
- `InspectShape()` composes existing shape, topology, adjacency, face, free-boundary, and optional mesh statistics into one managed snapshot.

Neither API makes application-specific pass/fail decisions. Tolerance thresholds, accepted surface types, and quality rules remain application policy.

## Batch face analysis

```csharp
using var model = new OcctModelingSession();
var shape = model.ImportStep(path);

var analysis = model.AnalyzeFaces(shape);

Console.WriteLine($"Faces: {analysis.FaceCount}");
Console.WriteLine($"Area: {analysis.TotalArea}");
Console.WriteLine($"Maximum face tolerance: {analysis.MaximumTolerance}");

foreach (var face in analysis.Faces)
{
    Console.WriteLine($"{face.SurfaceType}: {face.Area}");
}
```

Each `OcctFaceAnalysisInfo` contains:

- source `Face`;
- `SurfaceType`;
- topological `Orientation`;
- surface `Area`;
- face `Tolerance`;
- `UvBounds`;
- axis-aligned `Bounds`;
- unique `EdgeCount`;
- `WireCount`;
- convenience flags `IsAnalytic` and `IsFreeform`.

`OcctFaceAnalysisResult` additionally exposes total area, maximum face tolerance, surface-type counts, and `GetFacesBySurfaceType()`.

The native bridge maps all faces once and fills one contiguous result array. This avoids repeatedly crossing P/Invoke for surface type, UV range, bounds, area, tolerance, and local topology on every face.

## Structured shape inspection

```csharp
var report = model.InspectShape(shape);

Console.WriteLine(report.IsValid);
Console.WriteLine(report.IsClosed);
Console.WriteLine(report.MaximumTolerance);
Console.WriteLine(report.TopologyCounts[OcctShapeType.Face]);
Console.WriteLine(report.EdgeAdjacency.NonManifoldEdges.Count);
Console.WriteLine(report.FaceAnalysis.SurfaceTypeCounts.Count);
```

`OcctShapeInspectionReport` contains:

- shape type, validity, closure and maximum tolerance;
- OCCT check-report text;
- shape bounds;
- topology counts;
- batch edge-to-face adjacency snapshot;
- batch face-analysis snapshot;
- optional strict free-boundary result;
- optional mesh node/triangle/meshed-face statistics.

This is a data snapshot, not a validator policy. For example, a free boundary can be intentional for a sheet body, and a high tolerance may be acceptable for one model unit system but unacceptable for another.

## Options and side effects

Default options are intentionally conservative:

```csharp
var options = OcctShapeInspectionOptions.Default;
// IncludeFreeBounds = true
// FreeBoundaryTolerance = 1e-7
// GenerateMeshStatistics = false
```

Free-boundary analysis is enabled by default because it does not require triangulation. Mesh statistics are disabled by default because requesting them calls the normal triangulation path and can populate/update OCCT triangulation caches.

Enable mesh statistics explicitly when required:

```csharp
var report = model.InspectShape(shape, new OcctShapeInspectionOptions
{
    IncludeFreeBounds = true,
    FreeBoundaryTolerance = 1e-6,
    GenerateMeshStatistics = true,
    MeshParameters = OcctModelMeshParameters.Default
});
```

If the caller already needs full mesh provenance, call `GetShapeMeshData()` directly and retain that result rather than generating mesh statistics only to discard the combined mesh.

## Recommended engineering workflow

For large imported models:

1. call `InspectShape()` without mesh statistics for a compact first-pass snapshot;
2. inspect `EdgeAdjacency`, `FreeBounds`, face types and tolerances;
3. apply project-specific acceptance rules in the application layer;
4. only triangulate when visualization, mesh statistics, picking, export, or downstream analysis actually requires it;
5. retain the raw report fields for auditability instead of only storing a Boolean pass/fail result.

## ABI design

Batch face inspection is isolated in:

- `OcctModelingFaceAnalysis.h`
- `OcctModelingFaceAnalysis.cpp`

It adds one ABI 3 function:

- `occt_model_shape_face_analysis`

`InspectShape()` itself is a managed composition API and does not add another native function.

## Verification

Cloud CI validates the C ABI/PInvoke contract and compiles the Native Smoke scenarios, but the project OCCT SDK is not available in cloud CI. Real execution is covered by `tests/OcctNet.Smoke/ShapeInspectionSmoke.cs` and must run through the local native gate:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```
