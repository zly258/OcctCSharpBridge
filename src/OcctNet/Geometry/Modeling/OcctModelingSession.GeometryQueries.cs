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
        CheckStatus(ModelNativeMethods.occt_model_edge_endpoints(_handle, edge.Id, out var start, out var end));
        return (start, end);
    }

    public OcctEdgeEvaluation EvaluateEdge(OcctModelShape edge, double normalizedParameter)
    {
        EnsureShape(edge);
        OcctGuard.UnitInterval(normalizedParameter, nameof(normalizedParameter));
        CheckStatus(ModelNativeMethods.occt_model_edge_point_at(_handle, edge.Id, normalizedParameter, out var point, out var tangent));
        return new OcctEdgeEvaluation(point, tangent);
    }

    public double GetEdgeLength(OcctModelShape edge)
    {
        EnsureShape(edge);
        CheckStatus(ModelNativeMethods.occt_model_edge_length(_handle, edge.Id, out var result));
        return result;
    }


    public double GetEdgeLengthAtParameter(OcctModelShape edge, double parameter)
    {
        EnsureShape(edge);
        OcctGuard.Finite(parameter, nameof(parameter));
        CheckStatus(ModelNativeMethods.occt_model_edge_length_at_parameter(_handle, edge.Id, parameter, out var result));
        return result;
    }

    public double GetEdgeParameterAtLength(OcctModelShape edge, double length)
    {
        EnsureShape(edge);
        if (!double.IsFinite(length) || length < 0.0) throw new ArgumentOutOfRangeException(nameof(length));
        CheckStatus(ModelNativeMethods.occt_model_edge_point_at_length(_handle, edge.Id, length, out var parameter, out _, out _));
        return parameter;
    }

    public OcctEdgeEvaluation EvaluateEdgeAtLength(OcctModelShape edge, double length)
    {
        EnsureShape(edge);
        if (!double.IsFinite(length) || length < 0.0) throw new ArgumentOutOfRangeException(nameof(length));
        CheckStatus(ModelNativeMethods.occt_model_edge_point_at_length(_handle, edge.Id, length, out _, out var point, out var tangent));
        return new OcctEdgeEvaluation(point, tangent);
    }

    public OcctEdgeEvaluation EvaluateEdgeAtLengthRatio(OcctModelShape edge, double ratio)
    {
        EnsureShape(edge);
        OcctGuard.UnitInterval(ratio, nameof(ratio));
        return EvaluateEdgeAtLength(edge, GetEdgeLength(edge) * ratio);
    }

    public IReadOnlyList<OcctPoint3d> SampleEdge(OcctModelShape edge, int count)
    {
        EnsureShape(edge);
        if (count < 2) throw new ArgumentOutOfRangeException(nameof(count));

        var points = new OcctPoint3d[count];
        CheckStatus(ModelNativeMethods.occt_model_edge_sample_points_snapshot_get(
            _handle, edge.Id, count, points, points.Length, out var required));
        if (required != count)
            throw new InvalidOperationException("Native edge sample count changed during snapshot copy.");
        return points;
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
        CheckStatus(ModelNativeMethods.occt_model_face_point_normal(_handle, face.Id, u, v, out var point, out var normal));
        return new OcctFaceEvaluation(point, normal);
    }
}
