using System.Runtime.InteropServices;

namespace OcctNet;

internal static class PresentationNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_clip_planes(
        IntPtr handle,
        long objectId,
        [In] NativeOcctViewClipPlane[] planes,
        int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_global_highlight_style(
        IntPtr handle,
        int kind,
        in NativeOcctHighlightStyleSettings settings);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_highlight_style(
        IntPtr handle,
        long objectId,
        int dynamic,
        in NativeOcctHighlightStyleSettings settings);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_clear_object_highlight_style(
        IntPtr handle,
        long objectId,
        int dynamic);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_reset_object_display_mode(IntPtr handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_display_mode(
        IntPtr handle,
        long objectId,
        out int hasOverride,
        out int displayMode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_auto_highlight(IntPtr handle, long objectId, int enabled);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_auto_highlight(IntPtr handle, long objectId, out int enabled);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_infinite_state(IntPtr handle, long objectId, int infinite);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_infinite_state(IntPtr handle, long objectId, out int infinite);
}
