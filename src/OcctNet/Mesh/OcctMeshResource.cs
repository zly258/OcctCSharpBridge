namespace OcctNet;

/// <summary>
/// Owns an immutable native mesh snapshot independently from a modeling session registry.
/// </summary>
public sealed class OcctMeshResource : IDisposable
{
    private readonly OcctMeshSafeHandle _handle;

    internal OcctMeshResource(OcctMeshSafeHandle handle)
    {
        _handle = handle;
    }

    public bool IsDisposed => _handle.IsClosed || _handle.IsInvalid;

    public int NodeCount
    {
        get
        {
            var (nodeCount, _) = GetCounts();
            return nodeCount;
        }
    }

    public int TriangleCount
    {
        get
        {
            var (_, triangleCount) = GetCounts();
            return triangleCount;
        }
    }

    public OcctMesh GetMesh()
    {
        var (nodeCount, triangleCount) = GetCounts();
        var nativeNodes = new NativeModelMeshNode[nodeCount];
        var triangles = new OcctModelMeshTriangle[triangleCount];

        var status = ModelNativeMethods.occt_mesh_nodes_copy(
            _handle,
            nativeNodes,
            nativeNodes.Length,
            out var writtenNodes);
        ThrowIfFailed(status, nameof(GetMesh));
        if (writtenNodes != nodeCount)
            throw new InvalidOperationException("Native mesh-node count changed during bulk copy.");

        status = ModelNativeMethods.occt_mesh_triangles_copy(
            _handle,
            triangles,
            triangles.Length,
            out var writtenTriangles);
        ThrowIfFailed(status, nameof(GetMesh));
        if (writtenTriangles != triangleCount)
            throw new InvalidOperationException("Native mesh-triangle count changed during bulk copy.");

        var nodes = new OcctModelMeshNode[nodeCount];
        for (var index = 0; index < nodeCount; index++)
            nodes[index] = nativeNodes[index].ToManaged();
        return new OcctMesh(nodes, triangles);
    }

    public void Dispose()
    {
        _handle.Dispose();
    }

    private (int NodeCount, int TriangleCount) GetCounts()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        var status = ModelNativeMethods.occt_mesh_get_counts(
            _handle,
            out var nodeCount,
            out var triangleCount);
        ThrowIfFailed(status, nameof(GetCounts));
        return (nodeCount, triangleCount);
    }

    private static void ThrowIfFailed(OcctStatus status, string operation)
    {
        if (status != OcctStatus.Ok)
            throw new OcctException("Unable to access the owned native mesh.", status, operation);
    }
}
