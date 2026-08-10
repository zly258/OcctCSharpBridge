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

        var count = ModelNativeMethods.occt_model_intersect_edges(
            _handle,
            firstEdge.Id,
            secondEdge.Id,
            tolerance);
        if (count < 0) throw CreateException();
        if (count == 0) return Array.Empty<OcctEdgeIntersection>();

        var native = new NativeModelEdgeIntersection[count];
        var copied = ModelNativeMethods.occt_model_edge_intersections_copy(_handle, native, native.Length);
        if (copied < 0) throw CreateException();
        if (copied != count)
            throw new InvalidOperationException("Native edge-intersection count changed during bulk copy.");

        var result = new OcctEdgeIntersection[copied];
        for (var index = 0; index < copied; index++)
            result[index] = native[index].ToManaged();
        return result;
    }
}
