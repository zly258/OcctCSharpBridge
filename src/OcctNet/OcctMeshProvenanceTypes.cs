namespace OcctNet;

/// <summary>
/// Describes the contiguous node and triangle ranges contributed by one source face
/// to a combined shape mesh.
/// </summary>
public readonly struct OcctShapeMeshFaceRange
{
    internal OcctShapeMeshFaceRange(
        OcctModelShape face,
        int nodeStart,
        int nodeCount,
        int triangleStart,
        int triangleCount)
    {
        Face = face;
        NodeStart = nodeStart;
        NodeCount = nodeCount;
        TriangleStart = triangleStart;
        TriangleCount = triangleCount;
    }

    public OcctModelShape Face { get; }
    public int NodeStart { get; }
    public int NodeCount { get; }
    public int TriangleStart { get; }
    public int TriangleCount { get; }
    public int NodeEndExclusive => checked(NodeStart + NodeCount);
    public int TriangleEndExclusive => checked(TriangleStart + TriangleCount);

    public bool ContainsNode(int nodeIndex) =>
        nodeIndex >= NodeStart && nodeIndex < NodeEndExclusive;

    public bool ContainsTriangle(int triangleIndex) =>
        triangleIndex >= TriangleStart && triangleIndex < TriangleEndExclusive;
}

/// <summary>
/// Combined shape triangulation plus stable ranges that map mesh nodes and triangles
/// back to the source OCCT faces that contributed them.
/// </summary>
public sealed class OcctShapeMeshData
{
    internal OcctShapeMeshData(
        OcctMesh mesh,
        OcctShapeMeshFaceRange[] faceRanges)
    {
        Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        FaceRanges = Array.AsReadOnly((OcctShapeMeshFaceRange[])faceRanges.Clone());
    }

    public OcctMesh Mesh { get; }
    public IReadOnlyList<OcctShapeMeshFaceRange> FaceRanges { get; }
    public int FaceCount => FaceRanges.Count;
    public int NodeCount => Mesh.Nodes.Count;
    public int TriangleCount => Mesh.Triangles.Count;

    public OcctShapeMeshFaceRange GetFaceRange(int faceIndex)
    {
        if ((uint)faceIndex >= (uint)FaceRanges.Count)
            throw new ArgumentOutOfRangeException(nameof(faceIndex));
        return FaceRanges[faceIndex];
    }

    public bool TryGetFaceForTriangle(int triangleIndex, out OcctModelShape face)
    {
        if ((uint)triangleIndex >= (uint)TriangleCount)
        {
            face = default;
            return false;
        }

        var low = 0;
        var high = FaceRanges.Count - 1;
        while (low <= high)
        {
            var middle = low + ((high - low) / 2);
            var range = FaceRanges[middle];
            if (triangleIndex < range.TriangleStart)
            {
                high = middle - 1;
                continue;
            }
            if (triangleIndex >= range.TriangleEndExclusive)
            {
                low = middle + 1;
                continue;
            }

            face = range.Face;
            return true;
        }

        face = default;
        return false;
    }

    public OcctModelShape GetFaceForTriangle(int triangleIndex)
    {
        if (!TryGetFaceForTriangle(triangleIndex, out var face))
            throw new ArgumentOutOfRangeException(nameof(triangleIndex));
        return face;
    }
}
