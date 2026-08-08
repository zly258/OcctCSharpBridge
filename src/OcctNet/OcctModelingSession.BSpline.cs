namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctBSplineCurveData GetBSplineCurveData(OcctModelShape edge)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_bspline_info(_handle, edge.Id, out var info));
        if (info.Degree < 1 || info.PoleCount < 2 || info.KnotCount < 2)
            throw new InvalidOperationException("Native B-Spline metadata is invalid.");

        var poles = new OcctPoint3d[info.PoleCount];
        var weights = new double[info.PoleCount];
        for (var index = 0; index < poles.Length; index++)
        {
            Check(ModelNativeMethods.occt_model_edge_bspline_pole_at(
                _handle,
                edge.Id,
                index,
                out poles[index],
                out weights[index]));
            EnsureValidPole(poles[index], weights[index], "curve");
        }

        var knots = new double[info.KnotCount];
        var multiplicities = new int[info.KnotCount];
        for (var index = 0; index < knots.Length; index++)
        {
            Check(ModelNativeMethods.occt_model_edge_bspline_knot_at(
                _handle,
                edge.Id,
                index,
                out knots[index],
                out multiplicities[index]));
            EnsureValidKnot(knots, multiplicities, index, "curve");
        }

        return new OcctBSplineCurveData(
            info.Degree,
            info.Rational != 0,
            info.Periodic != 0,
            poles,
            weights,
            knots,
            multiplicities);
    }

    public OcctBSplineSurfaceData GetBSplineSurfaceData(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_bspline_info(_handle, face.Id, out var info));
        if (info.UDegree < 1 || info.VDegree < 1 ||
            info.UPoleCount < 2 || info.VPoleCount < 2 ||
            info.UKnotCount < 2 || info.VKnotCount < 2)
        {
            throw new InvalidOperationException("Native B-Spline surface metadata is invalid.");
        }

        var poleCount = checked(info.UPoleCount * info.VPoleCount);
        var poles = new OcctPoint3d[poleCount];
        var weights = new double[poleCount];
        for (var uIndex = 0; uIndex < info.UPoleCount; uIndex++)
        {
            for (var vIndex = 0; vIndex < info.VPoleCount; vIndex++)
            {
                var flatIndex = checked(uIndex * info.VPoleCount + vIndex);
                Check(ModelNativeMethods.occt_model_face_bspline_pole_at(
                    _handle,
                    face.Id,
                    uIndex,
                    vIndex,
                    out poles[flatIndex],
                    out weights[flatIndex]));
                EnsureValidPole(poles[flatIndex], weights[flatIndex], "surface");
            }
        }

        var uKnots = new double[info.UKnotCount];
        var uMultiplicities = new int[info.UKnotCount];
        for (var index = 0; index < uKnots.Length; index++)
        {
            Check(ModelNativeMethods.occt_model_face_bspline_u_knot_at(
                _handle,
                face.Id,
                index,
                out uKnots[index],
                out uMultiplicities[index]));
            EnsureValidKnot(uKnots, uMultiplicities, index, "surface U");
        }

        var vKnots = new double[info.VKnotCount];
        var vMultiplicities = new int[info.VKnotCount];
        for (var index = 0; index < vKnots.Length; index++)
        {
            Check(ModelNativeMethods.occt_model_face_bspline_v_knot_at(
                _handle,
                face.Id,
                index,
                out vKnots[index],
                out vMultiplicities[index]));
            EnsureValidKnot(vKnots, vMultiplicities, index, "surface V");
        }

        return new OcctBSplineSurfaceData(
            info.UDegree,
            info.VDegree,
            info.URational != 0,
            info.VRational != 0,
            info.UPeriodic != 0,
            info.VPeriodic != 0,
            info.UPoleCount,
            info.VPoleCount,
            poles,
            weights,
            uKnots,
            uMultiplicities,
            vKnots,
            vMultiplicities);
    }

    private static void EnsureValidPole(OcctPoint3d pole, double weight, string context)
    {
        if (!pole.IsFinite || !double.IsFinite(weight) || weight <= 0)
            throw new InvalidOperationException($"Native B-Spline {context} pole data is invalid.");
    }

    private static void EnsureValidKnot(double[] knots, int[] multiplicities, int index, string context)
    {
        if (!double.IsFinite(knots[index]) || multiplicities[index] <= 0)
            throw new InvalidOperationException($"Native B-Spline {context} knot data is invalid.");
        if (index > 0 && knots[index] <= knots[index - 1])
            throw new InvalidOperationException($"Native B-Spline {context} knots must be strictly increasing.");
    }
}
