using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ViewerShapeNativeMethods
{
    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_type_get(OcctEngineSafeHandle handle, long shapeId, out int result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_validity_get(OcctEngineSafeHandle handle, long shapeId, out int result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_bounds_get(OcctEngineSafeHandle handle, long shapeId, out OcctBounds result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_linear_properties_get(OcctEngineSafeHandle handle, long shapeId, out OcctMassProperties result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_surface_properties_get(OcctEngineSafeHandle handle, long shapeId, out OcctMassProperties result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_volume_properties_get(OcctEngineSafeHandle handle, long shapeId, out OcctMassProperties result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_distance_get(OcctEngineSafeHandle handle, long firstId, long secondId, out OcctDistanceResult result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_topology_count_get(OcctEngineSafeHandle handle, long shapeId, int shapeType, out int result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_subshape_copy(OcctEngineSafeHandle handle, long shapeId, int shapeType, int index, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_copy(OcctEngineSafeHandle handle, long shapeId, int hideInput, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_translate_copy(OcctEngineSafeHandle handle, long shapeId, OcctVector3d value, int hideInput, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_rotate_copy(OcctEngineSafeHandle handle, long shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_scale_copy(OcctEngineSafeHandle handle, long shapeId, OcctPoint3d center, double factor, int hideInput, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_mirror_plane_copy(OcctEngineSafeHandle handle, long shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal, int hideInput, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_hash_get(OcctEngineSafeHandle handle, long shapeId, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_vertex_point_get(OcctEngineSafeHandle handle, long vertexId, out OcctPoint3d result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_edge_endpoints_get(OcctEngineSafeHandle handle, long edgeId, out OcctPoint3d start, out OcctPoint3d end);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_edge_evaluate(OcctEngineSafeHandle handle, long edgeId, double normalizedParameter, out OcctPoint3d point, out OcctVector3d tangent);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_edge_project_point(OcctEngineSafeHandle handle, long edgeId, OcctPoint3d sourcePoint, out OcctEdgeProjectionResult result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_edge_curve_type_get(OcctEngineSafeHandle handle, long edgeId, out int result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_face_surface_type_get(OcctEngineSafeHandle handle, long faceId, out int result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_face_uv_bounds_get(OcctEngineSafeHandle handle, long faceId, out OcctUvBounds result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_face_evaluate(OcctEngineSafeHandle handle, long faceId, double u, double v, out OcctPoint3d point, out OcctVector3d normal);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_face_project_point(OcctEngineSafeHandle handle, long faceId, OcctPoint3d sourcePoint, out OcctFaceProjectionResult result);
}
