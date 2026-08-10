using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_project_point_on_edge(IntPtr handle, long edgeId, OcctPoint3d point, out OcctModelProjectionResult result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_project_point_on_face(IntPtr handle, long faceId, OcctPoint3d point, out OcctModelProjectionResult result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_ray_intersections(IntPtr handle, long shapeId, OcctPoint3d origin, OcctVector3d direction, double minimumParameter, double maximumParameter, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_ray_hit_count(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_ray_hit_at(IntPtr handle, int index, out NativeModelRayHit result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_ray_hits_copy(IntPtr handle, [Out] NativeModelRayHit[] results, int capacity);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_classify_point(IntPtr handle, long solidId, OcctPoint3d point, double tolerance);
}
