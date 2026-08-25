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

    /// <summary>Returns the current triangulation sizes for one face.</summary>
    public (int VertexCount, int TriangleCount) GetFaceMeshCounts(OcctModelShape face)
    {
        EnsureShape(face);
        CheckStatus(ModelNativeMethods.occt_model_face_mesh_nodes_snapshot_get(
            _handle, face.Id, null, 0, out var vertexCount));
        CheckStatus(ModelNativeMethods.occt_model_face_mesh_triangles_snapshot_get(
            _handle, face.Id, null, 0, out var triangleCount));
        return (vertexCount, triangleCount);
    }

    /// <summary>
    /// Copies one face triangulation directly into caller-provided buffers.
    /// </summary>
    public unsafe (int VerticesWritten, int TrianglesWritten) CopyFaceMesh(
        OcctModelShape face,
        Span<OcctMeshVertex> vertices,
        Span<OcctModelMeshTriangle> triangles)
    {
        var (vertexCount, triangleCount) = GetFaceMeshCounts(face);
        if (vertices.Length < vertexCount)
            throw new ArgumentException("Vertex destination is smaller than the face mesh.", nameof(vertices));
        if (triangles.Length < triangleCount)
            throw new ArgumentException("Triangle destination is smaller than the face mesh.", nameof(triangles));

        fixed (OcctMeshVertex* vertexPointer = vertices)
        fixed (OcctModelMeshTriangle* trianglePointer = triangles)
        {
            CheckStatus(ModelNativeMethods.FaceMeshVerticesCopyToPointer(
                _handle, face.Id, vertexPointer, vertices.Length, out var verticesWritten));
            CheckStatus(ModelNativeMethods.FaceMeshTrianglesCopyToPointer(
                _handle, face.Id, trianglePointer, triangles.Length, out var trianglesWritten));
            if (verticesWritten != vertexCount || trianglesWritten != triangleCount)
                throw new InvalidOperationException("Native face mesh changed during direct buffer copy.");
            return (verticesWritten, trianglesWritten);
        }
    }

    public OcctMesh GetFaceMesh(OcctModelShape face)
    {
        var (vertexCount, triangleCount) = GetFaceMeshCounts(face);
        var vertices = new OcctMeshVertex[vertexCount];
        var triangles = new OcctModelMeshTriangle[triangleCount];
        CopyFaceMesh(face, vertices, triangles);

        var nodes = new OcctModelMeshNode[vertexCount];
        for (var index = 0; index < vertexCount; index++)
            nodes[index] = vertices[index].ToManaged();
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

        var faces = GetSubshapes(shape, OcctShapeType.Face);
        var counts = new (int VertexCount, int TriangleCount)[faces.Count];
        var totalVertices = 0;
        var totalTriangles = 0;
        for (var index = 0; index < faces.Count; index++)
        {
            counts[index] = GetFaceMeshCounts(faces[index]);
            totalVertices = checked(totalVertices + counts[index].VertexCount);
            totalTriangles = checked(totalTriangles + counts[index].TriangleCount);
        }

        var vertices = new OcctMeshVertex[totalVertices];
        var triangles = new OcctModelMeshTriangle[totalTriangles];
        var ranges = new OcctShapeMeshFaceRange[faces.Count];
        var vertexStart = 0;
        var triangleStart = 0;

        for (var faceIndex = 0; faceIndex < faces.Count; faceIndex++)
        {
            var face = faces[faceIndex];
            var count = counts[faceIndex];
            CopyFaceMesh(
                face,
                vertices.AsSpan(vertexStart, count.VertexCount),
                triangles.AsSpan(triangleStart, count.TriangleCount));

            var triangleEnd = triangleStart + count.TriangleCount;
            for (var triangleIndex = triangleStart; triangleIndex < triangleEnd; triangleIndex++)
            {
                triangles[triangleIndex].Node1 += vertexStart;
                triangles[triangleIndex].Node2 += vertexStart;
                triangles[triangleIndex].Node3 += vertexStart;
            }

            ranges[faceIndex] = new OcctShapeMeshFaceRange(
                face,
                vertexStart,
                count.VertexCount,
                triangleStart,
                count.TriangleCount);
            vertexStart += count.VertexCount;
            triangleStart = triangleEnd;
        }

        var nodes = new OcctModelMeshNode[totalVertices];
        for (var index = 0; index < totalVertices; index++)
            nodes[index] = vertices[index].ToManaged();

        return new OcctShapeMeshData(new OcctMesh(nodes, triangles), ranges);
    }
}
