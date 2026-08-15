namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape MakePlanarFace(
        OcctModelShape outerWire,
        IEnumerable<OcctModelShape>? innerWires = null)
    {
        EnsureShape(outerWire);
        var holes = innerWires?.ToArray() ?? Array.Empty<OcctModelShape>();
        foreach (var hole in holes) EnsureShape(hole);
        var holeIds = holes.Select(hole => hole.Id).ToArray();
        var status = ModelNativeMethods.occt_model_make_face_with_holes(
            _handle,
            outerWire.Id,
            holeIds,
            holeIds.Length,
            out var result);
        return CheckShape(status, result);
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

        var status = ModelNativeMethods.occt_model_trim_edge(
            _handle,
            edge.Id,
            firstParameter,
            lastParameter,
            out var result);
        return CheckShape(status, result);
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

        var status = ModelNativeMethods.occt_model_offset_wire(
            _handle,
            wire.Id,
            offset,
            altitude,
            (int)joinType,
            openResult ? 1 : 0,
            out var result);
        return CheckShape(status, result);
    }
}
