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
        Check(ModelNativeMethods.occt_model_mesh(_handle, shape.Id, in native));
    }

    public void ClearTriangulation(OcctModelShape shape)
    {
        EnsureShape(shape);
        Check(ModelNativeMethods.occt_model_clear_mesh(_handle, shape.Id));
    }

    public OcctMesh GetFaceMesh(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_mesh_counts(
            _handle,
            face.Id,
            out var nodeCount,
            out var triangleCount));

        var nodes = new OcctModelMeshNode[nodeCount];
        var triangles = new OcctModelMeshTriangle[triangleCount];
        for (var index = 0; index < nodeCount; index++)
        {
            Check(ModelNativeMethods.occt_model_face_mesh_node(
                _handle,
                face.Id,
                index,
                out var native));
            nodes[index] = native.ToManaged();
        }

        for (var index = 0; index < triangleCount; index++)
        {
            Check(ModelNativeMethods.occt_model_face_mesh_triangle(
                _handle,
                face.Id,
                index,
                out triangles[index]));
        }

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
