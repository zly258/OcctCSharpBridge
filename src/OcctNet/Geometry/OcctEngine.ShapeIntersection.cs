namespace OcctNet;

public sealed partial class OcctEngine
{
    public IReadOnlyList<OcctEdgeIntersection> IntersectEdges(
        OcctShape firstEdge,
        OcctShape secondEdge,
        double tolerance = 1e-7)
    {
        EnsureShape(firstEdge);
        EnsureShape(secondEdge);
        if (!double.IsFinite(tolerance) || tolerance < 0.0)
            throw new ArgumentOutOfRangeException(
                nameof(tolerance),
                "Intersection tolerance must be finite and non-negative.");
        EnsureInitialized();

        var status =
            ViewerShapeNativeMethods.occt_engine_shape_intersect_edges_snapshot_get(
                _handle,
                firstEdge.Id,
                secondEdge.Id,
                tolerance,
                null,
                0,
                out var count);
        if (status != OcctStatus.Ok)
            throw CreateException();
        if (count == 0)
            return Array.Empty<OcctEdgeIntersection>();
        if (count < 0)
            throw new InvalidOperationException(
                "Native edge intersection count is invalid.");

        var native = new NativeModelEdgeIntersection[count];
        status =
            ViewerShapeNativeMethods.occt_engine_shape_intersect_edges_snapshot_get(
                _handle,
                firstEdge.Id,
                secondEdge.Id,
                tolerance,
                native,
                native.Length,
                out var required);
        if (status != OcctStatus.Ok)
            throw CreateException();
        if (required != count)
            throw new InvalidOperationException(
                "Native edge intersection count changed during snapshot copy.");

        var result = new OcctEdgeIntersection[count];
        for (var index = 0; index < count; index++)
        {
            result[index] = native[index].ToManaged();
            ValidateIntersection(result[index]);
        }
        return result;
    }

    private static void ValidateIntersection(
        OcctEdgeIntersection value)
    {
        if (!Enum.IsDefined(value.Kind) ||
            !value.StartPoint.IsFinite ||
            !value.EndPoint.IsFinite ||
            !double.IsFinite(value.FirstParameterStart) ||
            !double.IsFinite(value.FirstParameterEnd) ||
            !double.IsFinite(value.SecondParameterStart) ||
            !double.IsFinite(value.SecondParameterEnd))
        {
            throw new InvalidOperationException(
                "Native edge intersection returned invalid geometry.");
        }
    }
}
