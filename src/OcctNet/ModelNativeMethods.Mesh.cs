using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_mesh(OcctModelingSafeHandle handle, long shapeId, in NativeModelMeshParameters parameters);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_clear_mesh(OcctModelingSafeHandle handle, long shapeId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_mesh_nodes_copy(OcctModelingSafeHandle handle, long faceId, [Out] NativeModelMeshNode[]? results, int capacity);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_mesh_triangles_copy(OcctModelingSafeHandle handle, long faceId, [Out] OcctModelMeshTriangle[]? results, int capacity);
}
