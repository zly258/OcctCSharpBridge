using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_project_point_on_edge(IntPtr handle, long edgeId, OcctPoint3d point, out OcctModelProjectionResult result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_project_point_on_face(IntPtr handle, long faceId, OcctPoint3d point, out OcctModelProjectionResult result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_ray_intersections(IntPtr handle, long shapeId, OcctPoint3d origin, OcctVector3d direction, double minimumParameter, double maximumParameter, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_ray_hit_count(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_ray_hit_at(IntPtr handle, int index, out OcctModelRayHit result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_classify_point(IntPtr handle, long solidId, OcctPoint3d point, double tolerance);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_mesh(IntPtr handle, long shapeId, in OcctModelMeshParameters parameters);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_clear_mesh(IntPtr handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_face_mesh_counts(IntPtr handle, long faceId, out int nodeCount, out int triangleCount);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_face_mesh_node(IntPtr handle, long faceId, int index, out OcctModelMeshNode result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_face_mesh_triangle(IntPtr handle, long faceId, int index, out OcctModelMeshTriangle result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_import_file(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_import_step(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_import_iges(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_import_brep(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_import_stl(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_export_step(IntPtr handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_export_iges(IntPtr handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_export_brep(IntPtr handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_export_stl(IntPtr handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, double linearDeflection, double angularDeflection, int asciiMode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_display_in_engine(IntPtr engineHandle, IntPtr modelHandle, long shapeId, int fit);
}
