using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_project_point_on_edge(OcctModelingSafeHandle handle, long edgeId, OcctPoint3d point, out OcctModelProjectionResult result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_project_point_on_face(OcctModelingSafeHandle handle, long faceId, OcctPoint3d point, out OcctModelProjectionResult result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_ray_intersections(OcctModelingSafeHandle handle, long shapeId, OcctPoint3d origin, OcctVector3d direction, double minimumParameter, double maximumParameter, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_ray_hits_copy(OcctModelingSafeHandle handle, [Out] NativeModelRayHit[]? results, int capacity);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_classify_point(OcctModelingSafeHandle handle, long solidId, OcctPoint3d point, double tolerance);
}
