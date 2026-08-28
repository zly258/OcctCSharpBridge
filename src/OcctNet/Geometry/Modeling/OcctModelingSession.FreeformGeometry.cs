namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctParabolaGeometry GetParabolaGeometry(OcctModelShape edge)
    {
        EnsureShape(edge);
        CheckStatus(ModelNativeMethods.occt_model_edge_parabola_geometry(_handle, edge.Id, out var result));
        return result;
    }

    public OcctHyperbolaGeometry GetHyperbolaGeometry(OcctModelShape edge)
    {
        EnsureShape(edge);
        CheckStatus(ModelNativeMethods.occt_model_edge_hyperbola_geometry(_handle, edge.Id, out var result));
        return result;
    }

    public OcctBezierCurveData GetBezierCurveData(OcctModelShape edge)
    {
        EnsureShape(edge);
        CheckStatus(ModelNativeMethods.occt_model_edge_bezier_info(_handle, edge.Id, out var info));
        if (info.Degree < 1 || info.PoleCount < 2)
            throw new InvalidOperationException("Native Bezier curve metadata is invalid.");

        var poles = new OcctPoint3d[info.PoleCount];
        var weights = new double[info.PoleCount];
        CheckStatus(ModelNativeMethods.occt_model_edge_bezier_poles_snapshot_get(
            _handle, edge.Id, poles, weights, poles.Length, out var required));
        if (required != poles.Length)
            throw new InvalidOperationException("Native Bezier curve pole count changed during snapshot copy.");

        for (var index = 0; index < poles.Length; ++index)
        {
            if (!poles[index].IsFinite || !double.IsFinite(weights[index]) || weights[index] <= 0)
                throw new InvalidOperationException("Native Bezier curve pole data is invalid.");
        }

        return new OcctBezierCurveData(
            info.Degree,
            info.Rational != 0,
            info.Closed != 0,
            poles,
            weights);
    }

    public OcctBezierSurfaceData GetBezierSurfaceData(OcctModelShape face)
    {
        EnsureShape(face);
        CheckStatus(ModelNativeMethods.occt_model_face_bezier_info(_handle, face.Id, out var info));
        if (info.UDegree < 1 || info.VDegree < 1 || info.UPoleCount < 2 || info.VPoleCount < 2)
            throw new InvalidOperationException("Native Bezier surface metadata is invalid.");

        var count = checked(info.UPoleCount * info.VPoleCount);
        var poles = new OcctPoint3d[count];
        var weights = new double[count];
        CheckStatus(ModelNativeMethods.occt_model_face_bezier_poles_snapshot_get(
            _handle, face.Id, poles, weights, count, out var required));
        if (required != count)
            throw new InvalidOperationException("Native Bezier surface pole count changed during snapshot copy.");

        for (var index = 0; index < count; ++index)
        {
            if (!poles[index].IsFinite || !double.IsFinite(weights[index]) || weights[index] <= 0)
                throw new InvalidOperationException("Native Bezier surface pole data is invalid.");
        }

        return new OcctBezierSurfaceData(
            info.UDegree,
            info.VDegree,
            info.UPoleCount,
            info.VPoleCount,
            info.URational != 0,
            info.VRational != 0,
            poles,
            weights);
    }

    public OcctExtrusionSurfaceGeometry GetExtrusionSurfaceGeometry(OcctModelShape face)
    {
        EnsureShape(face);
        CheckStatus(ModelNativeMethods.occt_model_face_extrusion_geometry(_handle, face.Id, out var result));
        return result;
    }

    public OcctRevolutionSurfaceGeometry GetRevolutionSurfaceGeometry(OcctModelShape face)
    {
        EnsureShape(face);
        CheckStatus(ModelNativeMethods.occt_model_face_revolution_geometry(_handle, face.Id, out var result));
        return result;
    }

    public OcctOffsetSurfaceGeometry GetOffsetSurfaceGeometry(OcctModelShape face)
    {
        EnsureShape(face);
        CheckStatus(ModelNativeMethods.occt_model_face_offset_geometry(_handle, face.Id, out var result));
        return result;
    }
}
