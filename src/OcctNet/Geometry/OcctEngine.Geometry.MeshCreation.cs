using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape MakeTriangulatedMesh(
        IEnumerable<OcctPoint3d> vertices,
        IEnumerable<int> triangleIndices)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(triangleIndices);
        return MakeTriangulatedMesh(vertices.ToArray(), triangleIndices.ToArray());
    }

    public OcctShape MakeTriangulatedMesh(
        IReadOnlyList<OcctPoint3d> vertices,
        IReadOnlyList<int> triangleIndices)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(triangleIndices);

        var vertexArray = vertices as OcctPoint3d[] ?? vertices.ToArray();
        var indexArray = triangleIndices as int[] ?? triangleIndices.ToArray();
        OcctGuard.AtLeast(vertexArray.Length, 3, nameof(vertices));
        if (indexArray.Length < 3 || indexArray.Length % 3 != 0)
            throw new ArgumentException("Triangle indices must contain one or more complete triples.", nameof(triangleIndices));

        foreach (var vertex in vertexArray)
            OcctGuard.Finite(vertex, nameof(vertices));
        for (var index = 0; index < indexArray.Length; index++)
        {
            if ((uint)indexArray[index] >= (uint)vertexArray.Length)
                throw new ArgumentOutOfRangeException(nameof(triangleIndices), $"Triangle index {indexArray[index]} is outside the vertex buffer.");
        }

        EnsureInitialized();
        var vertexPin = GCHandle.Alloc(vertexArray, GCHandleType.Pinned);
        var indexPin = GCHandle.Alloc(indexArray, GCHandleType.Pinned);
        try
        {
            var status = ViewerGeometryCreationNativeMethods.occt_engine_shape_triangulated_mesh_create(
                _handle,
                vertexPin.AddrOfPinnedObject(),
                vertexArray.Length,
                indexPin.AddrOfPinnedObject(),
                indexArray.Length,
                out var result);
            return GeometryResult(status, result);
        }
        finally
        {
            indexPin.Free();
            vertexPin.Free();
        }
    }
}
