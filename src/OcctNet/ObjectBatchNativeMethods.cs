using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_color(
        OcctEngineSafeHandle handle,
        [In] long[] objectIds,
        int count,
        double r,
        double g,
        double b);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_transparency(
        OcctEngineSafeHandle handle,
        [In] long[] objectIds,
        int count,
        double transparency);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_visible(
        OcctEngineSafeHandle handle,
        [In] long[] objectIds,
        int count,
        int visible);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_display_mode(
        OcctEngineSafeHandle handle,
        [In] long[] objectIds,
        int count,
        int displayMode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_line_width(
        OcctEngineSafeHandle handle,
        [In] long[] objectIds,
        int count,
        double width);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_material(
        OcctEngineSafeHandle handle,
        [In] long[] objectIds,
        int count,
        int material);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_redisplay_objects(
        OcctEngineSafeHandle handle,
        [In] long[] objectIds,
        int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_select_objects(
        OcctEngineSafeHandle handle,
        [In] long[] objectIds,
        int count,
        int appendSelection);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_object_is_visible(OcctEngineSafeHandle handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_object_is_selected(OcctEngineSafeHandle handle, long objectId);
}
