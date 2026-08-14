using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_fit_objects(OcctEngineSafeHandle handle, [In] long[] objectIds, int count, double margin);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_zup_view(OcctEngineSafeHandle handle, int orientation, int fitAll);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_screen_to_ray(OcctEngineSafeHandle handle, int x, int y, out OcctProjectionRay result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_zoom_at_point(OcctEngineSafeHandle handle, int x, int y, double delta);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_select_all_visible(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_invert_selection(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_hide_selected(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_automatic_highlight(OcctEngineSafeHandle handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_msaa_samples(OcctEngineSafeHandle handle, int samples);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_render_resolution_scale(OcctEngineSafeHandle handle, double scale);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_render_resolution(OcctEngineSafeHandle handle, double dpi);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_rendering_method(OcctEngineSafeHandle handle, int method);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_shadows_enabled(OcctEngineSafeHandle handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_immediate_update(OcctEngineSafeHandle handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_frustum_culling(OcctEngineSafeHandle handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_face_boundaries_visible(OcctEngineSafeHandle handle, int visible, int applyExisting);
}
