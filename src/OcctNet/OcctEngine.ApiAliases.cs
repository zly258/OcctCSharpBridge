namespace OcctNet;

/// <summary>
/// Canonical naming aliases shared with <see cref="OcctModelingSession"/>.
/// Existing shorter names remain available for source compatibility.
/// </summary>
public sealed partial class OcctEngine
{
    public OcctBounds GetShapeBounds(OcctShape shape) => GetBounds(shape);

    public OcctDistanceResult GetShapeDistance(OcctShape first, OcctShape second) => Distance(first, second);

    public OcctShape GetSubshapeAt(OcctShape shape, OcctShapeType type, int index) => GetSubshape(shape, type, index);

    public OcctEdgeEvaluation EvaluateEdgeNormalized(OcctShape edge, double normalizedParameter) =>
        EvaluateEdge(edge, normalizedParameter);

    public OcctCurveType GetEdgeCurveType(OcctShape edge) => GetCurveType(edge);

    public OcctSurfaceType GetFaceSurfaceType(OcctShape face) => GetSurfaceType(face);

    public OcctUvBounds GetFaceUvBounds(OcctShape face) => GetUvBounds(face);

    public OcctFaceEvaluation EvaluateFaceAtParameters(OcctShape face, double u, double v) =>
        EvaluateFace(face, u, v);
}
