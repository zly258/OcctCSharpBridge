using System.Runtime.CompilerServices;
using OcctNet;

internal static class EdgeAdjacencySmoke
{
    [ModuleInitializer]
    internal static void Run()
    {
        using var model = new OcctModelingSession();

        var box = model.MakeBox(100, 80, 60);
        var boxAdjacency = model.AnalyzeEdgeAdjacency(box);
        if (boxAdjacency.EdgeCount != 12)
            throw new InvalidOperationException($"Box edge count is {boxAdjacency.EdgeCount}, expected 12.");
        if (boxAdjacency.Entries.Any(entry => entry.AdjacentFaceCount != 2))
            throw new InvalidOperationException("A box edge did not have exactly two distinct adjacent faces.");
        if (boxAdjacency.ManifoldInteriorEdges.Count != 12 ||
            boxAdjacency.BoundaryCandidates.Count != 0 ||
            boxAdjacency.NonManifoldEdges.Count != 0)
        {
            throw new InvalidOperationException("Box edge adjacency classification is inconsistent.");
        }

        var face = model.MakePlanarFace(model.MakeRectangleWire(120, 80));
        var faceAdjacency = model.AnalyzeEdgeAdjacency(face);
        if (faceAdjacency.EdgeCount != 4 || faceAdjacency.BoundaryCandidates.Count != 4)
            throw new InvalidOperationException("Rectangle face boundary adjacency classification is inconsistent.");
        if (faceAdjacency.Entries.Any(entry => entry.AdjacentFaceCount != 1))
            throw new InvalidOperationException("Rectangle face edge did not have exactly one distinct adjacent face.");

        var compatibilityCandidates = model.GetBoundaryEdgeCandidates(face);
        if (compatibilityCandidates.Count != faceAdjacency.BoundaryCandidates.Count)
            throw new InvalidOperationException("Convenience boundary classification differs from batched adjacency analysis.");
    }
}
