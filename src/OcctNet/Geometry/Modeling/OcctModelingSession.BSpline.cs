namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctBSplineCurveData GetBSplineCurveData(OcctModelShape edge)
    {
        EnsureShape(edge);
        CheckStatus(ModelNativeMethods.occt_model_edge_bspline_info(_handle, edge.Id, out var info));
        if (info.Degree < 1 || info.PoleCount < 2 || info.KnotCount < 2)
            throw new InvalidOperationException("Native B-Spline metadata is invalid.");

        var poles = new OcctPoint3d[info.PoleCount];
        var weights = new double[info.PoleCount];
        CheckStatus(ModelNativeMethods.occt_model_edge_bspline_poles_snapshot_get(
            _handle, edge.Id, poles, weights, poles.Length, out var poleCount));
        if (poleCount != poles.Length)
            throw new InvalidOperationException("Native B-Spline curve pole count changed during snapshot copy.");

        var knots = new double[info.KnotCount];
        var multiplicities = new int[info.KnotCount];
        CheckStatus(ModelNativeMethods.occt_model_edge_bspline_knots_snapshot_get(
            _handle, edge.Id, knots, multiplicities, knots.Length, out var knotCount));
        if (knotCount != knots.Length)
            throw new InvalidOperationException("Native B-Spline curve knot count changed during snapshot copy.");

        for (var index = 0; index < poles.Length; ++index)
            EnsureValidPole(poles[index], weights[index], "curve");
        for (var index = 0; index < knots.Length; ++index)
            EnsureValidKnot(knots, multiplicities, index, "curve");

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
        CheckStatus(ModelNativeMethods.occt_model_face_bspline_info(_handle, face.Id, out var info));
        if (info.UDegree < 1 || info.VDegree < 1 ||
            info.UPoleCount < 2 || info.VPoleCount < 2 ||
            info.UKnotCount < 2 || info.VKnotCount < 2)
        {
            throw new InvalidOperationException("Native B-Spline surface metadata is invalid.");
        }

        var poleCount = checked(info.UPoleCount * info.VPoleCount);
        var poles = new OcctPoint3d[poleCount];
        var weights = new double[poleCount];
        CheckStatus(ModelNativeMethods.occt_model_face_bspline_poles_snapshot_get(
            _handle, face.Id, poles, weights, poleCount, out var copiedPoleCount));
        if (copiedPoleCount != poleCount)
            throw new InvalidOperationException("Native B-Spline surface pole count changed during snapshot copy.");

        var uKnots = new double[info.UKnotCount];
        var uMultiplicities = new int[info.UKnotCount];
        CheckStatus(ModelNativeMethods.occt_model_face_bspline_u_knots_snapshot_get(
            _handle, face.Id, uKnots, uMultiplicities, uKnots.Length, out var copiedUKnotCount));
        if (copiedUKnotCount != uKnots.Length)
            throw new InvalidOperationException("Native B-Spline surface U-knot count changed during snapshot copy.");

        var vKnots = new double[info.VKnotCount];
        var vMultiplicities = new int[info.VKnotCount];
        CheckStatus(ModelNativeMethods.occt_model_face_bspline_v_knots_snapshot_get(
            _handle, face.Id, vKnots, vMultiplicities, vKnots.Length, out var copiedVKnotCount));
        if (copiedVKnotCount != vKnots.Length)
            throw new InvalidOperationException("Native B-Spline surface V-knot count changed during snapshot copy.");

        for (var index = 0; index < poleCount; ++index)
            EnsureValidPole(poles[index], weights[index], "surface");
        for (var index = 0; index < uKnots.Length; ++index)
            EnsureValidKnot(uKnots, uMultiplicities, index, "surface U");
        for (var index = 0; index < vKnots.Length; ++index)
            EnsureValidKnot(vKnots, vMultiplicities, index, "surface V");

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
