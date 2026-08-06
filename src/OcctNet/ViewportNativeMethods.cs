using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_fit_objects(IntPtr handle, [In] long[] objectIds, int count, double margin);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_zup_view(IntPtr handle, int orientation, int fitAll);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_screen_to_ray(IntPtr handle, int x, int y, out OcctProjectionRay result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_zoom_at_point(IntPtr handle, int x, int y, double delta);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_select_all_visible(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_invert_selection(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_hide_selected(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_automatic_highlight(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_msaa_samples(IntPtr handle, int samples);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_render_resolution_scale(IntPtr handle, double scale);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_render_resolution(IntPtr handle, double dpi);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_rendering_method(IntPtr handle, int method);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_shadows_enabled(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_immediate_update(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_frustum_culling(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_face_boundaries_visible(IntPtr handle, int visible, int applyExisting);
}
