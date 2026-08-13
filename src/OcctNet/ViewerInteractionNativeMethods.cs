using System.Runtime.InteropServices;

namespace OcctNet;

internal static class ViewerInteractionNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_add_point_pixmap(
        IntPtr handle,
        OcctPoint3d position,
        int width,
        int height,
        [In] byte[] pixels,
        int pixelCount,
        int pixelFormat);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_point_pixmap_style(
        IntPtr handle,
        long pointId,
        int width,
        int height,
        [In] byte[] pixels,
        int pixelCount,
        int pixelFormat);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_z_layer(IntPtr handle, long objectId, int layer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_z_layer(IntPtr handle, [In] long[] objectIds, int count, int layer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_z_layer(IntPtr handle, long objectId, out int layer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_triedron_options(IntPtr handle, in NativeOcctTriedronOptions options);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_view_cube_options(IntPtr handle, in NativeOcctViewCubeOptions options);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_face_boundary_style(
        IntPtr handle,
        long shapeId,
        int visible,
        double r,
        double g,
        double b,
        double width);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_face_boundary_styles(
        IntPtr handle,
        [In] long[] shapeIds,
        int count,
        int visible,
        double r,
        double g,
        double b,
        double width);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_default_face_boundary_style(
        IntPtr handle,
        int visible,
        double r,
        double g,
        double b,
        double width,
        int applyExisting);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_detected_hit_detail(
        IntPtr handle,
        out NativeOcctSelectionHitDetail result,
        out int hasHit);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_detect_at(
        IntPtr handle,
        int x,
        int y,
        int maxHits,
        [Out] NativeOcctSelectionHitDetail[] items,
        int capacity,
        out int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_indexed_vertex_point(IntPtr handle, long ownerId, int vertexIndex, out OcctPoint3d result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_indexed_edge_endpoints(IntPtr handle, long ownerId, int edgeIndex, out OcctPoint3d start, out OcctPoint3d end);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_indexed_edge_point_at(
        IntPtr handle,
        long ownerId,
        int edgeIndex,
        double normalizedParameter,
        out OcctPoint3d point,
        out OcctVector3d tangent);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_indexed_face_point_normal(
        IntPtr handle,
        long ownerId,
        int faceIndex,
        double u,
        double v,
        out OcctPoint3d point,
        out OcctVector3d normal);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_indexed_face_center(IntPtr handle, long ownerId, int faceIndex, out OcctPoint3d result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_transforms(
        IntPtr handle,
        [In] NativeOcctObjectTransformUpdate[] updates,
        int count);
}
