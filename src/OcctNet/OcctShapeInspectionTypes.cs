namespace OcctNet;

public struct OcctShapeInspectionOptions
{
    public bool IncludeFreeBounds { get; set; }
    public double FreeBoundaryTolerance { get; set; }
    public bool GenerateMeshStatistics { get; set; }
    public OcctModelMeshParameters MeshParameters { get; set; }

    public static OcctShapeInspectionOptions Default => new()
    {
        IncludeFreeBounds = true,
        FreeBoundaryTolerance = 1e-7,
        GenerateMeshStatistics = false,
        MeshParameters = OcctModelMeshParameters.Default
    };
}

public sealed class OcctShapeInspectionReport
{
    internal OcctShapeInspectionReport(
        OcctModelShape shape,
        OcctShapeType shapeType,
        bool isValid,
        bool isClosed,
        double maximumTolerance,
        string checkReport,
        OcctBounds bounds,
        IReadOnlyDictionary<OcctShapeType, int> topologyCounts,
        OcctEdgeAdjacencyResult edgeAdjacency,
        OcctFaceAnalysisResult faceAnalysis,
        OcctFreeBoundsResult? freeBounds,
        int? meshNodeCount,
        int? meshTriangleCount,
        int? meshedFaceCount)
    {
        Shape = shape;
        ShapeType = shapeType;
        IsValid = isValid;
        IsClosed = isClosed;
        MaximumTolerance = maximumTolerance;
        CheckReport = checkReport;
        Bounds = bounds;
        TopologyCounts = topologyCounts;
        EdgeAdjacency = edgeAdjacency;
        FaceAnalysis = faceAnalysis;
        FreeBounds = freeBounds;
        MeshNodeCount = meshNodeCount;
        MeshTriangleCount = meshTriangleCount;
        MeshedFaceCount = meshedFaceCount;
    }

    public OcctModelShape Shape { get; }
    public OcctShapeType ShapeType { get; }
    public bool IsValid { get; }
    public bool IsClosed { get; }
    public double MaximumTolerance { get; }
    public string CheckReport { get; }
    public OcctBounds Bounds { get; }
    public IReadOnlyDictionary<OcctShapeType, int> TopologyCounts { get; }
    public OcctEdgeAdjacencyResult EdgeAdjacency { get; }
    public OcctFaceAnalysisResult FaceAnalysis { get; }
    public OcctFreeBoundsResult? FreeBounds { get; }
    public int? MeshNodeCount { get; }
    public int? MeshTriangleCount { get; }
    public int? MeshedFaceCount { get; }
    public bool IncludesFreeBounds => FreeBounds is not null;
    public bool IncludesMeshStatistics => MeshNodeCount.HasValue;
}
