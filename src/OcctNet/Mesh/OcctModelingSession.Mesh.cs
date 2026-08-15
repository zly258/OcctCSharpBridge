namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public void Triangulate(
        OcctModelShape shape,
        OcctModelMeshParameters? parameters = null)
    {
        EnsureShape(shape);
        var actual = parameters ?? OcctModelMeshParameters.Default;
        OcctGuard.Positive(actual.LinearDeflection, nameof(actual.LinearDeflection));
        OcctGuard.Positive(actual.AngularDeflection, nameof(actual.AngularDeflection));
        OcctGuard.NonNegative(actual.MinimumSize, nameof(actual.MinimumSize));
        var native = actual.ToNative();
        CheckStatus(ModelNativeMethods.occt_model_mesh(_handle, shape.Id, in native));
    }

    public void ClearTriangulation(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_clear_mesh(_handle, shape.Id));
    }

    public OcctMesh GetFaceMesh(OcctModelShape face)
    {
        EnsureShape(face);

        var status = ModelNativeMethods.occt_model_face_mesh_nodes_snapshot_get(
            _handle,
            face.Id,
            null,
            0,
            out var nodeCount);
        CheckStatus(status);

        status = ModelNativeMethods.occt_model_face_mesh_triangles_snapshot_get(
            _handle,
            face.Id,
            null,
            0,
            out var triangleCount);
        CheckStatus(status);

        var nativeNodes = new NativeModelMeshNode[nodeCount];
        var triangles = new OcctModelMeshTriangle[triangleCount];

        if (nodeCount > 0)
        {
            status = ModelNativeMethods.occt_model_face_mesh_nodes_snapshot_get(
                _handle,
                face.Id,
                nativeNodes,
                nativeNodes.Length,
                out var requiredNodes);
            CheckStatus(status);
            if (requiredNodes != nodeCount)
                throw new InvalidOperationException("Native mesh-node count changed during snapshot copy.");
        }

        if (triangleCount > 0)
        {
            status = ModelNativeMethods.occt_model_face_mesh_triangles_snapshot_get(
                _handle,
                face.Id,
                triangles,
                triangles.Length,
                out var requiredTriangles);
            CheckStatus(status);
            if (requiredTriangles != triangleCount)
                throw new InvalidOperationException("Native mesh-triangle count changed during snapshot copy.");
        }

        var nodes = new OcctModelMeshNode[nodeCount];
        for (var index = 0; index < nodeCount; index++)
            nodes[index] = nativeNodes[index].ToManaged();

        return new OcctMesh(nodes, triangles);
    }

    public OcctMesh GetShapeMesh(
        OcctModelShape shape,
        OcctModelMeshParameters? parameters = null) =>
        GetShapeMeshData(shape, parameters).Mesh;

    /// <summary>
    /// Builds one combined mesh while preserving the contiguous node and triangle ranges
    /// contributed by every source face.
    /// </summary>
    public OcctShapeMeshData GetShapeMeshData(
        OcctModelShape shape,
        OcctModelMeshParameters? parameters = null)
    {
        EnsureShape(shape);
        Triangulate(shape, parameters);

        var nodes = new List<OcctModelMeshNode>();
        var triangles = new List<OcctModelMeshTriangle>();
        var faces = GetSubshapes(shape, OcctShapeType.Face);
        var ranges = new List<OcctShapeMeshFaceRange>(faces.Count);

        foreach (var face in faces)
        {
            var faceMesh = GetFaceMesh(face);
            var nodeStart = nodes.Count;
            var triangleStart = triangles.Count;

            nodes.AddRange(faceMesh.Nodes);
            foreach (var triangle in faceMesh.Triangles)
            {
                triangles.Add(new OcctModelMeshTriangle
                {
                    Node1 = triangle.Node1 + nodeStart,
                    Node2 = triangle.Node2 + nodeStart,
                    Node3 = triangle.Node3 + nodeStart
                });
            }

            ranges.Add(new OcctShapeMeshFaceRange(
                face,
                nodeStart,
                faceMesh.Nodes.Count,
                triangleStart,
                faceMesh.Triangles.Count));
        }

        return new OcctShapeMeshData(
            new OcctMesh(nodes, triangles),
            ranges.ToArray());
    }
}
