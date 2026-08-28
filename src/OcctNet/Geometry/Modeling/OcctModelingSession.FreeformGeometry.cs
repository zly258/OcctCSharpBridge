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
        for (var index = 0; index < info.PoleCount; index++)
        {
            CheckStatus(ModelNativeMethods.occt_model_edge_bezier_pole_at(
                _handle, edge.Id, index, out poles[index], out weights[index]));
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
        for (var u = 0; u < info.UPoleCount; u++)
        {
            for (var v = 0; v < info.VPoleCount; v++)
            {
                var index = checked(u * info.VPoleCount + v);
                CheckStatus(ModelNativeMethods.occt_model_face_bezier_pole_at(
                    _handle, face.Id, u, v, out poles[index], out weights[index]));
                if (!poles[index].IsFinite || !double.IsFinite(weights[index]) || weights[index] <= 0)
                    throw new InvalidOperationException("Native Bezier surface pole data is invalid.");
            }
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
