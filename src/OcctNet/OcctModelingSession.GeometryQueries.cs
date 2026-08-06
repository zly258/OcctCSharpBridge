using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctPoint3d GetVertexPoint(OcctModelShape vertex)
    {
        EnsureShape(vertex);
        Check(ModelNativeMethods.occt_model_vertex_point(_handle, vertex.Id, out var result));
        return result;
    }

    public (OcctPoint3d Start, OcctPoint3d End) GetEdgeEndpoints(OcctModelShape edge)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_endpoints(
            _handle, edge.Id, out var start, out var end));
        return (start, end);
    }

    public OcctEdgeEvaluation EvaluateEdgeNormalized(
        OcctModelShape edge,
        double normalizedParameter)
    {
        EnsureShape(edge);
        if (normalizedParameter < 0.0 || normalizedParameter > 1.0)
            throw new ArgumentOutOfRangeException(
                nameof(normalizedParameter),
                "Normalized edge parameter must be in the range [0, 1].");
        Check(ModelNativeMethods.occt_model_edge_point_at(
            _handle,
            edge.Id,
            normalizedParameter,
            out var point,
            out var tangent));
        return new OcctEdgeEvaluation(point, tangent);
    }

    public OcctEdgeEvaluation EvaluateEdge(
        OcctModelShape edge,
        double normalizedParameter) => EvaluateEdgeNormalized(edge, normalizedParameter);

    public OcctCurveType GetEdgeCurveType(OcctModelShape edge)
    {
        EnsureShape(edge);
        return (OcctCurveType)ModelNativeMethods.occt_model_edge_curve_type(_handle, edge.Id);
    }

    public OcctCurveType GetCurveType(OcctModelShape edge) => GetEdgeCurveType(edge);

    public OcctSurfaceType GetFaceSurfaceType(OcctModelShape face)
    {
        EnsureShape(face);
        return (OcctSurfaceType)ModelNativeMethods.occt_model_face_surface_type(_handle, face.Id);
    }

    public OcctSurfaceType GetSurfaceType(OcctModelShape face) => GetFaceSurfaceType(face);

    public OcctUvBounds GetFaceUvBounds(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_uv_bounds(_handle, face.Id, out var result));
        return result;
    }

    public OcctUvBounds GetUvBounds(OcctModelShape face) => GetFaceUvBounds(face);

    public OcctFaceEvaluation EvaluateFaceAtParameters(
        OcctModelShape face,
        double u,
        double v)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_point_normal(
            _handle, face.Id, u, v, out var point, out var normal));
        return new OcctFaceEvaluation(point, normal);
    }

    public OcctFaceEvaluation EvaluateFace(OcctModelShape face, double u, double v) =>
        EvaluateFaceAtParameters(face, u, v);
}
