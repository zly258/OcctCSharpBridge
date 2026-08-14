using System.Runtime.InteropServices;

namespace OcctNet;

internal static class ViewerInteractionExtensionsNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_selection_mode_active(
        OcctEngineSafeHandle handle,
        long objectId,
        int mode,
        int active,
        int concurrency,
        int force);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_selection_sensitivity(
        OcctEngineSafeHandle handle,
        long objectId,
        int mode,
        int sensitivity);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_display_priority(
        OcctEngineSafeHandle handle,
        long objectId,
        int priority);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_display_priority(
        OcctEngineSafeHandle handle,
        [In] long[] objectIds,
        int count,
        int priority);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_display_priority(
        OcctEngineSafeHandle handle,
        long objectId,
        out int priority);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_transform_persistence_3d(
        OcctEngineSafeHandle handle,
        long objectId,
        int mode,
        OcctPoint3d anchor);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_transform_persistence_2d(
        OcctEngineSafeHandle handle,
        long objectId,
        int mode,
        int position,
        int offsetX,
        int offsetY);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_clear_object_transform_persistence(
        OcctEngineSafeHandle handle,
        long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_transform_persistence(
        OcctEngineSafeHandle handle,
        long objectId,
        out NativeOcctTransformPersistenceState result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_view_clip_planes(
        OcctEngineSafeHandle handle,
        [In] NativeOcctViewClipPlane[] planes,
        int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_view_clip_plane_limit(
        OcctEngineSafeHandle handle,
        out int limit);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_update_points(
        OcctEngineSafeHandle handle,
        [In] NativeOcctPointStateUpdate[] updates,
        int count);
}
