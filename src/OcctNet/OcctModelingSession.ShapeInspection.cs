namespace OcctNet;

public sealed partial class OcctModelingSession
{
    /// <summary>
    /// Builds a structured inspection snapshot without making application-specific pass/fail decisions.
    /// Mesh statistics are generated only when explicitly requested.
    /// </summary>
    public OcctShapeInspectionReport InspectShape(
        OcctModelShape shape,
        OcctShapeInspectionOptions? options = null)
    {
        EnsureShape(shape);
        var actual = options ?? OcctShapeInspectionOptions.Default;

        if (actual.IncludeFreeBounds)
        {
            OcctGuard.Finite(actual.FreeBoundaryTolerance, nameof(actual.FreeBoundaryTolerance));
            if (actual.FreeBoundaryTolerance <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(actual.FreeBoundaryTolerance),
                    actual.FreeBoundaryTolerance,
                    "Free-boundary tolerance must be greater than zero.");
        }

        var topologyCounts = GetTopologyCounts(shape)
            .ToDictionary(static pair => pair.Key, static pair => pair.Value);
        var edgeAdjacency = AnalyzeEdgeAdjacency(shape);
        var faceAnalysis = AnalyzeFaces(shape);
        var freeBounds = actual.IncludeFreeBounds
            ? AnalyzeFreeBounds(shape, actual.FreeBoundaryTolerance)
            : null;

        int? meshNodeCount = null;
        int? meshTriangleCount = null;
        int? meshedFaceCount = null;
        if (actual.GenerateMeshStatistics)
        {
            var meshData = GetShapeMeshData(shape, actual.MeshParameters);
            meshNodeCount = meshData.Mesh.Nodes.Count;
            meshTriangleCount = meshData.Mesh.Triangles.Count;
            meshedFaceCount = meshData.FaceRanges.Count;
        }

        return new OcctShapeInspectionReport(
            shape,
            GetShapeType(shape),
            IsShapeValid(shape),
            IsShapeClosed(shape),
            GetShapeMaximumTolerance(shape),
            GetShapeCheckReport(shape),
            GetShapeBounds(shape),
            topologyCounts,
            edgeAdjacency,
            faceAnalysis,
            freeBounds,
            meshNodeCount,
            meshTriangleCount,
            meshedFaceCount);
    }
}
