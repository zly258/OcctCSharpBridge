namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public bool IsSameShape(OcctModelShape first, OcctModelShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        return ModelNativeMethods.occt_model_shape_is_same(_handle, first.Id, second.Id) != 0;
    }

    public bool IsPartnerShape(OcctModelShape first, OcctModelShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        return ModelNativeMethods.occt_model_shape_is_partner(_handle, first.Id, second.Id) != 0;
    }

    public OcctOrientedBounds GetShapeOrientedBounds(OcctModelShape shape, bool optimal = false)
    {
        EnsureShape(shape);
        Check(ModelNativeMethods.occt_model_shape_oriented_bounds(
            _handle,
            shape.Id,
            optimal ? 1 : 0,
            out var result));
        return result;
    }

    public OcctModelShape MakePlanarFace(
        OcctModelShape outerWire,
        IEnumerable<OcctModelShape>? innerWires = null)
    {
        EnsureShape(outerWire);
        var holes = innerWires?.ToArray() ?? Array.Empty<OcctModelShape>();
        foreach (var hole in holes) EnsureShape(hole);
        var holeIds = holes.Select(hole => hole.Id).ToArray();
        return CheckShape(ModelNativeMethods.occt_model_make_face_with_holes(
            _handle,
            outerWire.Id,
            holeIds,
            holeIds.Length));
    }

    public OcctModelShape TrimEdge(
        OcctModelShape edge,
        double firstParameter,
        double lastParameter)
    {
        EnsureShape(edge);
        OcctGuard.Finite(firstParameter, nameof(firstParameter));
        OcctGuard.Finite(lastParameter, nameof(lastParameter));
        if (firstParameter >= lastParameter)
            throw new ArgumentException("firstParameter must be less than lastParameter.", nameof(firstParameter));
        return CheckShape(ModelNativeMethods.occt_model_trim_edge(
            _handle,
            edge.Id,
            firstParameter,
            lastParameter));
    }

    public OcctModelShape OffsetWire(
        OcctModelShape wire,
        double offset,
        double altitude = 0,
        OcctJoinType joinType = OcctJoinType.Arc,
        bool openResult = false)
    {
        EnsureShape(wire);
        OcctGuard.Finite(offset, nameof(offset));
        if (Math.Abs(offset) <= 1e-15)
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be non-zero.");
        OcctGuard.Finite(altitude, nameof(altitude));
        if (!Enum.IsDefined(joinType)) throw new ArgumentOutOfRangeException(nameof(joinType));
        return CheckShape(ModelNativeMethods.occt_model_offset_wire(
            _handle,
            wire.Id,
            offset,
            altitude,
            (int)joinType,
            openResult ? 1 : 0));
    }

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
            if (!poles[index].IsFinite || !double.IsFinite(weights[index]) || weights[index] <= 0)
                throw new InvalidOperationException("Native B-Spline pole data is invalid.");
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
            if (!double.IsFinite(knots[index]) || multiplicities[index] <= 0)
                throw new InvalidOperationException("Native B-Spline knot data is invalid.");
            if (index > 0 && knots[index] <= knots[index - 1])
                throw new InvalidOperationException("Native B-Spline knots must be strictly increasing.");
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
}
