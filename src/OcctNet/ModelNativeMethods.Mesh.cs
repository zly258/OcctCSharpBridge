using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_mesh(IntPtr handle, long shapeId, in NativeModelMeshParameters parameters);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_clear_mesh(IntPtr handle, long shapeId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_mesh_nodes_copy(IntPtr handle, long faceId, [Out] NativeModelMeshNode[]? results, int capacity);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_mesh_triangles_copy(IntPtr handle, long faceId, [Out] OcctModelMeshTriangle[]? results, int capacity);
}
