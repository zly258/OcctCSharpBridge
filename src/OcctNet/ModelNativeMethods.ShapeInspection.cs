using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_shape_hash(OcctModelingSafeHandle handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_shape_type(OcctModelingSafeHandle handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_shape_orientation(OcctModelingSafeHandle handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_shape_is_closed(OcctModelingSafeHandle handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_shape_is_valid(OcctModelingSafeHandle handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern double occt_model_shape_tolerance(OcctModelingSafeHandle handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_shape_bounds(OcctModelingSafeHandle handle, long shapeId, out OcctBounds result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_shape_linear_properties(OcctModelingSafeHandle handle, long shapeId, out OcctMassProperties result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_shape_surface_properties(OcctModelingSafeHandle handle, long shapeId, out OcctMassProperties result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_shape_volume_properties(OcctModelingSafeHandle handle, long shapeId, out OcctMassProperties result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_shape_distance(OcctModelingSafeHandle handle, long firstId, long secondId, out OcctDistanceResult result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern IntPtr occt_model_check_report(OcctModelingSafeHandle handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_get_location(OcctModelingSafeHandle handle, long shapeId, out OcctModelLocation result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_set_location(OcctModelingSafeHandle handle, long shapeId, in OcctModelLocation location, int copyShape);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_vertex_point(OcctModelingSafeHandle handle, long vertexId, out OcctPoint3d result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_edge_endpoints(OcctModelingSafeHandle handle, long edgeId, out OcctPoint3d start, out OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_edge_point_at(OcctModelingSafeHandle handle, long edgeId, double normalizedParameter, out OcctPoint3d point, out OcctVector3d tangent);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_edge_curve_type(OcctModelingSafeHandle handle, long edgeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_face_surface_type(OcctModelingSafeHandle handle, long faceId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_face_uv_bounds(OcctModelingSafeHandle handle, long faceId, out OcctUvBounds result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_face_point_normal(OcctModelingSafeHandle handle, long faceId, double u, double v, out OcctPoint3d point, out OcctVector3d normal);
}
