using System.Runtime.InteropServices;

namespace OcctNet;

internal static class ViewerInteractionNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_add_point_pixmap(
        OcctEngineSafeHandle handle,
        OcctPoint3d position,
        int width,
        int height,
        [In] byte[] pixels,
        int pixelCount,
        int pixelFormat);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_point_pixmap_style(
        OcctEngineSafeHandle handle,
        long pointId,
        int width,
        int height,
        [In] byte[] pixels,
        int pixelCount,
        int pixelFormat);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_z_layer(OcctEngineSafeHandle handle, long objectId, int layer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_z_layer(OcctEngineSafeHandle handle, [In] long[] objectIds, int count, int layer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_z_layer(OcctEngineSafeHandle handle, long objectId, out int layer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_triedron_options(OcctEngineSafeHandle handle, in NativeOcctTriedronOptions options);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_view_cube_options(OcctEngineSafeHandle handle, in NativeOcctViewCubeOptions options);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_face_boundary_style(
        OcctEngineSafeHandle handle,
        long shapeId,
        int visible,
        double r,
        double g,
        double b,
        double width);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_face_boundary_styles(
        OcctEngineSafeHandle handle,
        [In] long[] shapeIds,
        int count,
        int visible,
        double r,
        double g,
        double b,
        double width);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_default_face_boundary_style(
        OcctEngineSafeHandle handle,
        int visible,
        double r,
        double g,
        double b,
        double width,
        int applyExisting);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_detected_hit_detail(
        OcctEngineSafeHandle handle,
        out NativeOcctSelectionHitDetail result,
        out int hasHit);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_detect_at(
        OcctEngineSafeHandle handle,
        int x,
        int y,
        int maxHits,
        [Out] NativeOcctSelectionHitDetail[] items,
        int capacity,
        out int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_indexed_vertex_point(OcctEngineSafeHandle handle, long ownerId, int vertexIndex, out OcctPoint3d result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_indexed_edge_endpoints(OcctEngineSafeHandle handle, long ownerId, int edgeIndex, out OcctPoint3d start, out OcctPoint3d end);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_indexed_edge_point_at(
        OcctEngineSafeHandle handle,
        long ownerId,
        int edgeIndex,
        double normalizedParameter,
        out OcctPoint3d point,
        out OcctVector3d tangent);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_indexed_face_point_normal(
        OcctEngineSafeHandle handle,
        long ownerId,
        int faceIndex,
        double u,
        double v,
        out OcctPoint3d point,
        out OcctVector3d normal);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_indexed_face_center(OcctEngineSafeHandle handle, long ownerId, int faceIndex, out OcctPoint3d result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_transforms(
        OcctEngineSafeHandle handle,
        [In] NativeOcctObjectTransformUpdate[] updates,
        int count);
}
