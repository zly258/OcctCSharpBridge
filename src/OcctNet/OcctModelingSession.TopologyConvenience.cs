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

    public IReadOnlyList<OcctModelShape> GetWireEdges(OcctModelShape wire) =>
        GetSubshapes(wire, OcctShapeType.Edge);

    public IReadOnlyList<OcctModelShape> GetFaceEdges(OcctModelShape face) =>
        GetSubshapes(face, OcctShapeType.Edge);

    public IReadOnlyList<OcctModelShape> GetFaceVertices(OcctModelShape face) =>
        GetSubshapes(face, OcctShapeType.Vertex);

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
