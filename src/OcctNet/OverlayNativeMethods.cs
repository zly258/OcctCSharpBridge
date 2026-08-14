using System.Runtime.InteropServices;

namespace OcctNet;

internal static class OverlayNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_add_overlay_line(OcctEngineSafeHandle handle, OcctPoint3d start, OcctPoint3d end, int pattern, double width, double r, double g, double b);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_add_overlay_polyline(OcctEngineSafeHandle handle, [In] OcctPoint3d[] points, int count, int pattern, double width, double r, double g, double b);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_add_overlay_marker(OcctEngineSafeHandle handle, OcctPoint3d position, int marker, double scale, double r, double g, double b);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_add_overlay_text(OcctEngineSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, OcctPoint3d position, double height, double r, double g, double b, int zoomable, [MarshalAs(UnmanagedType.LPUTF8Str)] string fontName);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_update_overlay_line(OcctEngineSafeHandle handle, long overlayId, OcctPoint3d start, OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_update_overlay_polyline(OcctEngineSafeHandle handle, long overlayId, [In] OcctPoint3d[] points, int count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_update_overlay_marker(OcctEngineSafeHandle handle, long overlayId, OcctPoint3d position);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_update_overlay_text(OcctEngineSafeHandle handle, long overlayId, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, OcctPoint3d position);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_overlay_line_style(OcctEngineSafeHandle handle, long overlayId, int pattern, double width, double r, double g, double b);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_overlay_marker_style(OcctEngineSafeHandle handle, long overlayId, int marker, double scale, double r, double g, double b);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_overlay_text_style(OcctEngineSafeHandle handle, long overlayId, double height, double r, double g, double b, int zoomable, [MarshalAs(UnmanagedType.LPUTF8Str)] string fontName);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_overlay_primitive_type(OcctEngineSafeHandle handle, long overlayId, out int primitiveType);
}
