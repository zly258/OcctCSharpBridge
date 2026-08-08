using System.Runtime.CompilerServices;
using OcctNet;

internal static class ShapeMeshProvenanceSmoke
{
    [ModuleInitializer]
    internal static void Run()
    {
        using var model = new OcctModelingSession();
        var box = model.MakeBox(100, 80, 60);
        var data = model.GetShapeMeshData(box, new OcctModelMeshParameters
        {
            LinearDeflection = 0.5,
            AngularDeflection = 0.5,
            MinimumSize = 0.01,
            Relative = false,
            Parallel = false,
            InternalVertices = true,
            ControlSurfaceDeflection = true
        });

        if (data.FaceCount != 6)
            throw new InvalidOperationException($"Box mesh provenance has {data.FaceCount} face ranges, expected 6.");
        if (data.NodeCount <= 0 || data.TriangleCount <= 0)
            throw new InvalidOperationException("Box combined mesh is empty.");

        var expectedNodeStart = 0;
        var expectedTriangleStart = 0;
        foreach (var range in data.FaceRanges)
        {
            if (!range.Face.IsValid)
                throw new InvalidOperationException("Mesh provenance contains an invalid source face.");
            if (range.NodeStart != expectedNodeStart || range.TriangleStart != expectedTriangleStart)
                throw new InvalidOperationException("Mesh provenance ranges are not contiguous.");
            if (range.NodeCount <= 0 || range.TriangleCount <= 0)
                throw new InvalidOperationException("A box face contributed an empty mesh range.");

            var nodeFace = data.GetFaceForNode(range.NodeStart);
            if (!model.IsSameShape(range.Face, nodeFace))
                throw new InvalidOperationException("Node provenance did not resolve to the source face.");

            var triangleFace = data.GetFaceForTriangle(range.TriangleStart);
            if (!model.IsSameShape(range.Face, triangleFace))
                throw new InvalidOperationException("Triangle provenance did not resolve to the source face.");

            expectedNodeStart = range.NodeEndExclusive;
            expectedTriangleStart = range.TriangleEndExclusive;
        }

        if (expectedNodeStart != data.NodeCount || expectedTriangleStart != data.TriangleCount)
            throw new InvalidOperationException("Mesh provenance ranges do not cover the complete combined mesh.");
        if (data.TryGetFaceForNode(-1, out _) || data.TryGetFaceForNode(data.NodeCount, out _))
            throw new InvalidOperationException("Out-of-range node provenance lookup unexpectedly succeeded.");
        if (data.TryGetFaceForTriangle(-1, out _) || data.TryGetFaceForTriangle(data.TriangleCount, out _))
            throw new InvalidOperationException("Out-of-range triangle provenance lookup unexpectedly succeeded.");
    }
}
