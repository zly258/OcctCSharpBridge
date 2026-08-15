namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public IReadOnlyList<OcctEdgeIntersection> IntersectEdges(
        OcctModelShape firstEdge,
        OcctModelShape secondEdge,
        double tolerance = 1e-7)
    {
        EnsureShape(firstEdge);
        EnsureShape(secondEdge);
        OcctGuard.NonNegative(tolerance, nameof(tolerance));

        var status = ModelNativeMethods.occt_model_intersect_edges(
            _handle,
            firstEdge.Id,
            secondEdge.Id,
            tolerance,
            out var count);
        CheckStatus(status);
        if (count == 0) return Array.Empty<OcctEdgeIntersection>();

        var native = new NativeModelEdgeIntersection[count];
        status = ModelNativeMethods.occt_model_edge_intersections_snapshot_get(
            _handle,
            native,
            native.Length,
            out var required);
        CheckStatus(status);
        if (required != count)
            throw new InvalidOperationException("Native edge-intersection count changed during snapshot copy.");

        var result = new OcctEdgeIntersection[count];
        for (var index = 0; index < count; index++)
            result[index] = native[index].ToManaged();
        return result;
    }
}
