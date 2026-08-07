namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public void Mesh(OcctModelShape shape, OcctModelMeshParameters? parameters = null)
    {
        EnsureShape(shape);
        var actual = parameters ?? OcctModelMeshParameters.Default;
        Check(ModelNativeMethods.occt_model_mesh(_handle, shape.Id, in actual));
    }

    public void ClearMesh(OcctModelShape shape)
    {
        EnsureShape(shape);
        Check(ModelNativeMethods.occt_model_clear_mesh(_handle, shape.Id));
    }

    public OcctFaceMesh GetFaceMesh(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_mesh_counts(_handle, face.Id, out var nodeCount, out var triangleCount));
        var nodes = new OcctModelMeshNode[nodeCount];
        var triangles = new OcctModelMeshTriangle[triangleCount];
        for (var index = 0; index < nodeCount; index++)
            Check(ModelNativeMethods.occt_model_face_mesh_node(_handle, face.Id, index, out nodes[index]));
        for (var index = 0; index < triangleCount; index++)
            Check(ModelNativeMethods.occt_model_face_mesh_triangle(_handle, face.Id, index, out triangles[index]));
        return new OcctFaceMesh(nodes, triangles);
    }
}
