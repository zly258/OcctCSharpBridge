namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctFreeBoundsResult AnalyzeFreeBounds(
        OcctModelShape shape,
        double tolerance = 1e-7,
        bool splitClosed = true,
        bool splitOpen = true)
    {
        EnsureShape(shape);
        OcctGuard.Positive(tolerance, nameof(tolerance));

        const int closedKind = 0;
        const int openKind = 1;
        var splitClosedNative = splitClosed ? 1 : 0;
        var splitOpenNative = splitOpen ? 1 : 0;

        var status = ModelNativeMethods.occt_model_shape_free_bounds(
            _handle,
            shape.Id,
            tolerance,
            closedKind,
            splitClosedNative,
            splitOpenNative,
            out var closedId);
        var closedCompound = CheckShape(status, closedId);

        status = ModelNativeMethods.occt_model_shape_free_bounds(
            _handle,
            shape.Id,
            tolerance,
            openKind,
            splitClosedNative,
            splitOpenNative,
            out var openId);
        var openCompound = CheckShape(status, openId);

        return new OcctFreeBoundsResult(
            tolerance,
            GetWires(closedCompound),
            GetWires(openCompound));
    }

    /// <summary>
    /// Builds one native edge-to-face topology map and returns the adjacency count for every edge.
    /// Use this snapshot when several edge classifications are required for the same root shape.
    /// </summary>
    public OcctEdgeAdjacencyResult AnalyzeEdgeAdjacency(OcctModelShape root)
    {
        EnsureShape(root);
        CheckStatus(ModelNativeMethods.occt_model_shape_edge_adjacency_snapshot_get(
            _handle,
            root.Id,
            null,
            0,
            out var edgeCount));
        if (edgeCount == 0)
            return new OcctEdgeAdjacencyResult(root, Array.Empty<OcctEdgeAdjacencyInfo>());

        var nativeEntries = new NativeModelEdgeAdjacency[edgeCount];
        CheckStatus(ModelNativeMethods.occt_model_shape_edge_adjacency_snapshot_get(
            _handle,
            root.Id,
            nativeEntries,
            nativeEntries.Length,
            out var required));
        if (required != edgeCount)
            throw new InvalidOperationException($"Native edge adjacency count changed during analysis: expected {edgeCount}, returned {required}.");

        var entries = new OcctEdgeAdjacencyInfo[edgeCount];
        for (var index = 0; index < nativeEntries.Length; index++)
        {
            var native = nativeEntries[index];
            if (native.EdgeId <= 0 || native.AdjacentFaceCount < 0)
                throw new InvalidOperationException("Native edge adjacency data is invalid.");
            entries[index] = new OcctEdgeAdjacencyInfo(
                new OcctModelShape(native.EdgeId, _ownerId),
                native.AdjacentFaceCount);
        }

        return new OcctEdgeAdjacencyResult(root, entries);
    }
}
