namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public bool IsSameShape(OcctModelShape first, OcctModelShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        CheckStatus(ModelNativeMethods.occt_model_shape_is_same(
            _handle,
            first.Id,
            second.Id,
            out var result));
        return result != 0;
    }

    public bool IsPartnerShape(OcctModelShape first, OcctModelShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        CheckStatus(ModelNativeMethods.occt_model_shape_is_partner(
            _handle,
            first.Id,
            second.Id,
            out var result));
        return result != 0;
    }

    public OcctOrientedBounds GetShapeOrientedBounds(OcctModelShape shape, bool optimal = false)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_oriented_bounds(
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
}
