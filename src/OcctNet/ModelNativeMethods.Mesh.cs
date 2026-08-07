using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_mesh(IntPtr handle, long shapeId, in OcctModelMeshParameters parameters);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_clear_mesh(IntPtr handle, long shapeId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_mesh_counts(IntPtr handle, long faceId, out int nodeCount, out int triangleCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_mesh_node(IntPtr handle, long faceId, int index, out OcctModelMeshNode result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_mesh_triangle(IntPtr handle, long faceId, int index, out OcctModelMeshTriangle result);
}
