namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public IReadOnlyList<OcctModelShape> GetVertices(OcctModelShape shape) =>
        GetSubshapes(shape, OcctShapeType.Vertex);

    public IReadOnlyList<OcctModelShape> GetEdges(OcctModelShape shape) =>
        GetSubshapes(shape, OcctShapeType.Edge);

    public IReadOnlyList<OcctModelShape> GetWires(OcctModelShape shape) =>
        GetSubshapes(shape, OcctShapeType.Wire);

    public IReadOnlyList<OcctModelShape> GetFaces(OcctModelShape shape) =>
        GetSubshapes(shape, OcctShapeType.Face);

    public IReadOnlyList<OcctModelShape> GetShells(OcctModelShape shape) =>
        GetSubshapes(shape, OcctShapeType.Shell);

    public IReadOnlyList<OcctModelShape> GetSolids(OcctModelShape shape) =>
        GetSubshapes(shape, OcctShapeType.Solid);

    public IReadOnlyList<OcctModelShape> GetCompSolids(OcctModelShape shape) =>
        GetSubshapes(shape, OcctShapeType.CompSolid);

    public IReadOnlyList<OcctModelShape> GetCompounds(OcctModelShape shape) =>
        GetSubshapes(shape, OcctShapeType.Compound);

    public IReadOnlyList<OcctModelShape> GetEdgeVertices(OcctModelShape edge) =>
        GetSubshapes(edge, OcctShapeType.Vertex);

    public IReadOnlyList<OcctModelShape> GetFaceEdges(OcctModelShape face) =>
        GetSubshapes(face, OcctShapeType.Edge);

    public IReadOnlyList<OcctModelShape> GetFaceVertices(OcctModelShape face) =>
        GetSubshapes(face, OcctShapeType.Vertex);

    public IReadOnlyList<OcctModelShape> GetAdjacentFaces(OcctModelShape root, OcctModelShape edge) =>
        GetAncestors(root, edge, OcctShapeType.Face);

    public IReadOnlyList<OcctModelShape> GetIncidentEdges(OcctModelShape root, OcctModelShape vertex) =>
        GetAncestors(root, vertex, OcctShapeType.Edge);

    public IReadOnlyList<OcctModelShape> GetIncidentFaces(OcctModelShape root, OcctModelShape vertex) =>
        GetAncestors(root, vertex, OcctShapeType.Face);

    /// <summary>
    /// Returns edges that are referenced by exactly one distinct face in <paramref name="root"/>.
    /// These are useful free-boundary candidates, but periodic seam topology should be checked
    /// before treating every returned edge as an open geometric boundary.
    /// </summary>
    public IReadOnlyList<OcctModelShape> GetBoundaryEdgeCandidates(OcctModelShape root) =>
        GetEdgesByAdjacentFaceCount(root, 1, 1);

    public IReadOnlyList<OcctModelShape> GetManifoldInteriorEdges(OcctModelShape root) =>
        GetEdgesByAdjacentFaceCount(root, 2, 2);

    public IReadOnlyList<OcctModelShape> GetNonManifoldEdges(OcctModelShape root) =>
        GetEdgesByAdjacentFaceCount(root, 3, int.MaxValue);

    public IReadOnlyList<OcctModelShape> GetEdgesByAdjacentFaceCount(
        OcctModelShape root,
        int minimumFaceCount,
        int maximumFaceCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimumFaceCount);
        if (maximumFaceCount < minimumFaceCount)
            throw new ArgumentOutOfRangeException(nameof(maximumFaceCount), maximumFaceCount, "Maximum face count must be greater than or equal to the minimum face count.");

        return AnalyzeEdgeAdjacency(root)
            .GetEdgesByAdjacentFaceCount(minimumFaceCount, maximumFaceCount);
    }

    public IReadOnlyDictionary<OcctShapeType, int> GetTopologyCounts(OcctModelShape shape)
    {
        EnsureShape(shape);
        var result = new Dictionary<OcctShapeType, int>(8);
        foreach (var type in new[]
        {
            OcctShapeType.Compound,
            OcctShapeType.CompSolid,
            OcctShapeType.Solid,
            OcctShapeType.Shell,
            OcctShapeType.Face,
            OcctShapeType.Wire,
            OcctShapeType.Edge,
            OcctShapeType.Vertex
        })
        {
            result[type] = GetTopologyCount(shape, type);
        }

        return result;
    }
}
