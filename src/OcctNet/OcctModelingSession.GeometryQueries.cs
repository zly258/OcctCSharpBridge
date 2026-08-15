namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctPoint3d GetVertexPoint(OcctModelShape vertex)
    {
        EnsureShape(vertex);
        CheckStatus(ModelNativeMethods.occt_model_vertex_point(_handle, vertex.Id, out var result));
        return result;
    }

    public (OcctPoint3d Start, OcctPoint3d End) GetEdgeEndpoints(OcctModelShape edge)
    {
        EnsureShape(edge);
        CheckStatus(ModelNativeMethods.occt_model_edge_endpoints(
            _handle,
            edge.Id,
            out var start,
            out var end));
        return (start, end);
    }

    public OcctEdgeEvaluation EvaluateEdge(
        OcctModelShape edge,
        double normalizedParameter)
    {
        EnsureShape(edge);
        OcctGuard.UnitInterval(normalizedParameter, nameof(normalizedParameter));
        CheckStatus(ModelNativeMethods.occt_model_edge_point_at(
            _handle,
            edge.Id,
            normalizedParameter,
            out var point,
            out var tangent));
        return new OcctEdgeEvaluation(point, tangent);
    }

    public OcctCurveType GetEdgeCurveType(OcctModelShape edge)
    {
        EnsureShape(edge);
        CheckStatus(ModelNativeMethods.occt_model_edge_curve_type(_handle, edge.Id, out var result));
        return result;
    }

    public OcctSurfaceType GetFaceSurfaceType(OcctModelShape face)
    {
        EnsureShape(face);
        CheckStatus(ModelNativeMethods.occt_model_face_surface_type(_handle, face.Id, out var result));
        return result;
    }

    public OcctUvBounds GetFaceUvBounds(OcctModelShape face)
    {
        EnsureShape(face);
        CheckStatus(ModelNativeMethods.occt_model_face_uv_bounds(_handle, face.Id, out var result));
        return result;
    }

    public OcctFaceEvaluation EvaluateFace(OcctModelShape face, double u, double v)
    {
        EnsureShape(face);
        OcctGuard.Finite(u, nameof(u));
        OcctGuard.Finite(v, nameof(v));
        CheckStatus(ModelNativeMethods.occt_model_face_point_normal(
            _handle,
            face.Id,
            u,
            v,
            out var point,
            out var normal));
        return new OcctFaceEvaluation(point, normal);
    }
}
