using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_set_objects_color(
        IntPtr handle,
        [In] long[] objectIds,
        int count,
        double r,
        double g,
        double b);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_set_objects_transparency(
        IntPtr handle,
        [In] long[] objectIds,
        int count,
        double transparency);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_set_objects_visible(
        IntPtr handle,
        [In] long[] objectIds,
        int count,
        int visible);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_set_objects_display_mode(
        IntPtr handle,
        [In] long[] objectIds,
        int count,
        int displayMode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_set_objects_line_width(
        IntPtr handle,
        [In] long[] objectIds,
        int count,
        double width);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_set_objects_material(
        IntPtr handle,
        [In] long[] objectIds,
        int count,
        int material);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_redisplay_objects(
        IntPtr handle,
        [In] long[] objectIds,
        int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_select_objects(
        IntPtr handle,
        [In] long[] objectIds,
        int count,
        int appendSelection);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_object_is_visible(IntPtr handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_object_is_selected(IntPtr handle, long objectId);
}
