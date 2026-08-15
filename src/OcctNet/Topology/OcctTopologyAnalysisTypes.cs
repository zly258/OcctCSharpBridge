using System.Runtime.InteropServices;

namespace OcctNet;

/// <summary>
/// Read-only result of strict OCCT free-boundary analysis.
/// Closed and open wires are owned by the same modeling session as the analyzed shape.
/// </summary>
public sealed class OcctFreeBoundsResult
{
    internal OcctFreeBoundsResult(
        double tolerance,
        IReadOnlyList<OcctModelShape> closedWires,
        IReadOnlyList<OcctModelShape> openWires)
    {
        Tolerance = tolerance;
        ClosedWires = closedWires;
        OpenWires = openWires;
    }

    public double Tolerance { get; }
    public IReadOnlyList<OcctModelShape> ClosedWires { get; }
    public IReadOnlyList<OcctModelShape> OpenWires { get; }
    public int ClosedWireCount => ClosedWires.Count;
    public int OpenWireCount => OpenWires.Count;
    public int TotalWireCount => checked(ClosedWireCount + OpenWireCount);
    public bool HasFreeBounds => TotalWireCount > 0;
    public bool HasOpenFreeBounds => OpenWireCount > 0;
}

/// <summary>One edge and the number of distinct ancestor faces that reference it.</summary>
public readonly struct OcctEdgeAdjacencyInfo
{
    internal OcctEdgeAdjacencyInfo(OcctModelShape edge, int adjacentFaceCount)
    {
        Edge = edge;
        AdjacentFaceCount = adjacentFaceCount;
    }

    public OcctModelShape Edge { get; }
    public int AdjacentFaceCount { get; }
    public bool IsIsolated => AdjacentFaceCount == 0;
    public bool IsBoundaryCandidate => AdjacentFaceCount == 1;
    public bool IsManifoldInterior => AdjacentFaceCount == 2;
    public bool IsNonManifold => AdjacentFaceCount >= 3;
}

/// <summary>
/// Immutable snapshot of all edge-to-face adjacency counts for one root shape.
/// The native topology map is built once for the whole snapshot.
/// </summary>
public sealed class OcctEdgeAdjacencyResult
{
    internal OcctEdgeAdjacencyResult(OcctModelShape root, OcctEdgeAdjacencyInfo[] entries)
    {
        Root = root;
        Entries = Array.AsReadOnly((OcctEdgeAdjacencyInfo[])entries.Clone());
        IsolatedEdges = SelectEdges(entries, static entry => entry.IsIsolated);
        BoundaryCandidates = SelectEdges(entries, static entry => entry.IsBoundaryCandidate);
        ManifoldInteriorEdges = SelectEdges(entries, static entry => entry.IsManifoldInterior);
        NonManifoldEdges = SelectEdges(entries, static entry => entry.IsNonManifold);
    }

    public OcctModelShape Root { get; }
    public IReadOnlyList<OcctEdgeAdjacencyInfo> Entries { get; }
    public int EdgeCount => Entries.Count;
    public IReadOnlyList<OcctModelShape> IsolatedEdges { get; }
    public IReadOnlyList<OcctModelShape> BoundaryCandidates { get; }
    public IReadOnlyList<OcctModelShape> ManifoldInteriorEdges { get; }
    public IReadOnlyList<OcctModelShape> NonManifoldEdges { get; }
    public bool HasBoundaryCandidates => BoundaryCandidates.Count > 0;
    public bool HasNonManifoldEdges => NonManifoldEdges.Count > 0;

    public IReadOnlyList<OcctModelShape> GetEdgesByAdjacentFaceCount(int minimumFaceCount, int maximumFaceCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimumFaceCount);
        if (maximumFaceCount < minimumFaceCount)
            throw new ArgumentOutOfRangeException(nameof(maximumFaceCount), maximumFaceCount, "Maximum face count must be greater than or equal to the minimum face count.");

        return Entries
            .Where(entry => entry.AdjacentFaceCount >= minimumFaceCount && entry.AdjacentFaceCount <= maximumFaceCount)
            .Select(entry => entry.Edge)
            .ToArray();
    }

    private static IReadOnlyList<OcctModelShape> SelectEdges(
        IEnumerable<OcctEdgeAdjacencyInfo> entries,
        Func<OcctEdgeAdjacencyInfo, bool> predicate) =>
        entries.Where(predicate).Select(entry => entry.Edge).ToArray();
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelEdgeAdjacency
{
    internal long EdgeId;
    internal int AdjacentFaceCount;
}
