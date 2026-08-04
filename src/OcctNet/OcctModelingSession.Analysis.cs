namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelProjectionResult ProjectPointOnEdge(OcctModelShape edge, OcctPoint3d point)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_project_point_on_edge(_handle, edge.Id, point, out var result));
        return result;
    }

    public OcctModelProjectionResult ProjectPointOnFace(OcctModelShape face, OcctPoint3d point)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_project_point_on_face(_handle, face.Id, point, out var result));
        return result;
    }

    public IReadOnlyList<OcctModelRayHit> IntersectRay(
        OcctModelShape shape,
        OcctPoint3d origin,
        OcctVector3d direction,
        double minimumParameter = 0,
        double maximumParameter = 1e12,
        double tolerance = 1e-7)
    {
        EnsureShape(shape);
        var count = ModelNativeMethods.occt_model_ray_intersections(
            _handle,
            shape.Id,
            origin,
            direction,
            minimumParameter,
            maximumParameter,
            tolerance);
        if (count < 0) throw CreateException();
        var result = new OcctModelRayHit[count];
        for (var index = 0; index < count; index++)
        {
            Check(ModelNativeMethods.occt_model_ray_hit_at(_handle, index, out result[index]));
        }
        return result;
    }

    public OcctModelState ClassifyPoint(OcctModelShape solid, OcctPoint3d point, double tolerance = 1e-7)
    {
        EnsureShape(solid);
        return (OcctModelState)ModelNativeMethods.occt_model_classify_point(_handle, solid.Id, point, tolerance);
    }

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

    public OcctModelShape Import(string filePath)
    {
        ValidatePath(filePath);
        return CheckShape(ModelNativeMethods.occt_model_import_file(_handle, Path.GetFullPath(filePath)));
    }

    public OcctModelShape ImportStep(string filePath) => ImportSpecific(filePath, ModelNativeMethods.occt_model_import_step);
    public OcctModelShape ImportIges(string filePath) => ImportSpecific(filePath, ModelNativeMethods.occt_model_import_iges);
    public OcctModelShape ImportBrep(string filePath) => ImportSpecific(filePath, ModelNativeMethods.occt_model_import_brep);
    public OcctModelShape ImportStl(string filePath) => ImportSpecific(filePath, ModelNativeMethods.occt_model_import_stl);

    public void ExportStep(OcctModelShape shape, string filePath) => ExportShape(shape, filePath, ModelNativeMethods.occt_model_export_step);
    public void ExportIges(OcctModelShape shape, string filePath) => ExportShape(shape, filePath, ModelNativeMethods.occt_model_export_iges);
    public void ExportBrep(OcctModelShape shape, string filePath) => ExportShape(shape, filePath, ModelNativeMethods.occt_model_export_brep);

    public void ExportStl(
        OcctModelShape shape,
        string filePath,
        double linearDeflection = 0.1,
        double angularDeflection = 0.5,
        bool ascii = false)
    {
        EnsureShape(shape);
        ValidatePath(filePath);
        Check(ModelNativeMethods.occt_model_export_stl(
            _handle, shape.Id, Path.GetFullPath(filePath), linearDeflection, angularDeflection, ascii ? 1 : 0));
    }
}
