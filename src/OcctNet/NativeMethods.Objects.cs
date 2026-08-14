namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct OcctObjectDescriptorNative
{
    internal long ObjectId;
    internal int Kind;
}

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_object_count(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_object_descriptors(OcctEngineSafeHandle handle, [Out] OcctObjectDescriptorNative[]? items, int capacity, out int objectCount, out int shapeCount);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_object_exists(OcctEngineSafeHandle handle, long objectId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_object_kind(OcctEngineSafeHandle handle, long objectId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_object_name(OcctEngineSafeHandle handle, long objectId, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern IntPtr occt_get_object_name(OcctEngineSafeHandle handle, long objectId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_object_color(OcctEngineSafeHandle handle, long objectId, double r, double g, double b);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_object_transparency(OcctEngineSafeHandle handle, long objectId, double transparency);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_object_visible(OcctEngineSafeHandle handle, long objectId, int visible);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_object_display_mode(OcctEngineSafeHandle handle, long objectId, int displayMode);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_object_line_width(OcctEngineSafeHandle handle, long objectId, double width);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_object_material(OcctEngineSafeHandle handle, long objectId, int material);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_delete_objects(OcctEngineSafeHandle handle, [In] long[] objectIds, int count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_clear(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_shape_type(OcctEngineSafeHandle handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_shape_is_valid(OcctEngineSafeHandle handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_shape_bounds(OcctEngineSafeHandle handle, long shapeId, out OcctBounds result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_shape_linear_properties(OcctEngineSafeHandle handle, long shapeId, out OcctMassProperties result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_shape_surface_properties(OcctEngineSafeHandle handle, long shapeId, out OcctMassProperties result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_shape_volume_properties(OcctEngineSafeHandle handle, long shapeId, out OcctMassProperties result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_shape_distance(OcctEngineSafeHandle handle, long firstId, long secondId, out OcctDistanceResult result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_topology_count(OcctEngineSafeHandle handle, long shapeId, int shapeType);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_get_subshape(OcctEngineSafeHandle handle, long shapeId, int shapeType, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_copy_shape(OcctEngineSafeHandle handle, long shapeId, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_translate(OcctEngineSafeHandle handle, long shapeId, OcctVector3d vector, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_rotate(OcctEngineSafeHandle handle, long shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_scale(OcctEngineSafeHandle handle, long shapeId, OcctPoint3d center, double factor, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_mirror_plane(OcctEngineSafeHandle handle, long shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal, int hideInput);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_shape_hash(OcctEngineSafeHandle handle, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_vertex_point(OcctEngineSafeHandle handle, long vertexId, out OcctPoint3d result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_edge_endpoints(OcctEngineSafeHandle handle, long edgeId, out OcctPoint3d start, out OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_edge_point_at(OcctEngineSafeHandle handle, long edgeId, double normalizedParameter, out OcctPoint3d point, out OcctVector3d tangent);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_edge_curve_type(OcctEngineSafeHandle handle, long edgeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_face_surface_type(OcctEngineSafeHandle handle, long faceId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_face_uv_bounds(OcctEngineSafeHandle handle, long faceId, out OcctUvBounds result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_face_point_normal(OcctEngineSafeHandle handle, long faceId, double u, double v, out OcctPoint3d point, out OcctVector3d normal);
}
