using System.Runtime.CompilerServices;
using OcctNet;

internal static class ShapeInspectionSmoke
{
    [ModuleInitializer]
    internal static void Run()
    {
        using var model = new OcctModelingSession();
        var box = model.MakeBox(10, 20, 30);

        var faces = model.AnalyzeFaces(box);
        if (faces.FaceCount != 6)
            throw new InvalidOperationException($"Box face analysis count is {faces.FaceCount}, expected 6.");
        if (!faces.SurfaceTypeCounts.TryGetValue(OcctSurfaceType.Plane, out var planeCount) || planeCount != 6)
            throw new InvalidOperationException("Box face analysis did not classify all six faces as planes.");

        const double expectedArea = 2200.0;
        if (Math.Abs(faces.TotalArea - expectedArea) > 1e-6)
            throw new InvalidOperationException($"Box total face area is {faces.TotalArea}, expected {expectedArea}.");
        if (faces.Faces.Any(face =>
                face.EdgeCount != 4 ||
                face.WireCount != 1 ||
                face.Area <= 0 ||
                !face.Bounds.IsValid() ||
                !face.UvBounds.IsValid()))
        {
            throw new InvalidOperationException("Box contains invalid per-face analysis data.");
        }

        var inspection = model.InspectShape(box);
        if (!inspection.IsValid || !inspection.IsClosed)
            throw new InvalidOperationException("Box inspection validity/closure metadata is incorrect.");
        if (inspection.TopologyCounts[OcctShapeType.Face] != 6 ||
            inspection.TopologyCounts[OcctShapeType.Edge] != 12 ||
            inspection.TopologyCounts[OcctShapeType.Vertex] != 8)
        {
            throw new InvalidOperationException("Box inspection topology counts are incorrect.");
        }
        if (inspection.EdgeAdjacency.HasBoundaryCandidates || inspection.EdgeAdjacency.HasNonManifoldEdges)
            throw new InvalidOperationException("Closed box unexpectedly contains boundary or non-manifold edge candidates.");
        if (inspection.FaceAnalysis.FaceCount != 6 || !inspection.IncludesFreeBounds)
            throw new InvalidOperationException("Box inspection did not include the expected face/free-boundary snapshots.");
        if (inspection.IncludesMeshStatistics)
            throw new InvalidOperationException("Default shape inspection must not generate mesh statistics.");

        var meshInspection = model.InspectShape(box, new OcctShapeInspectionOptions
        {
            IncludeFreeBounds = false,
            FreeBoundaryTolerance = 1e-7,
            GenerateMeshStatistics = true,
            MeshParameters = OcctModelMeshParameters.Default
        });
        if (!meshInspection.IncludesMeshStatistics ||
            meshInspection.MeshNodeCount is null or <= 0 ||
            meshInspection.MeshTriangleCount is null or <= 0 ||
            meshInspection.MeshedFaceCount != 6 ||
            meshInspection.IncludesFreeBounds)
        {
            throw new InvalidOperationException("Explicit shape inspection mesh statistics are invalid.");
        }
    }
}
