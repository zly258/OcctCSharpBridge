namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_initialize(OcctEngineSafeHandle handle, IntPtr windowHandle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_resize(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_resize_surface(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_redraw(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_fit_all(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_fit_object(OcctEngineSafeHandle handle, long objectId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_window_fit(OcctEngineSafeHandle handle, int x1, int y1, int x2, int y2);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_view(OcctEngineSafeHandle handle, int orientation);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_projection(OcctEngineSafeHandle handle, int projectionType);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_perspective_fov(OcctEngineSafeHandle handle, double degrees);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_background(OcctEngineSafeHandle handle, double r, double g, double b);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_display_mode(OcctEngineSafeHandle handle, int displayMode);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_triedron_visible(OcctEngineSafeHandle handle, int visible);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_view_cube_visible(OcctEngineSafeHandle handle, int visible);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_computed_mode(OcctEngineSafeHandle handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_display_precision(OcctEngineSafeHandle handle, double deviationCoefficient, double deviationAngleDegrees, int applyExisting);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_default_material(OcctEngineSafeHandle handle, int material, int applyExisting);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_reset_scene_lighting(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_selection_tolerance(OcctEngineSafeHandle handle, int pixelTolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_dump_view(OcctEngineSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_screen_to_world(OcctEngineSafeHandle handle, int x, int y, out OcctPoint3d result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_world_to_screen(OcctEngineSafeHandle handle, OcctPoint3d point, out int x, out int y);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_move_to(OcctEngineSafeHandle handle, int x, int y);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_select(OcctEngineSafeHandle handle, int x, int y, int appendSelection);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_select_rectangle_ex(OcctEngineSafeHandle handle, int x1, int y1, int x2, int y2, int appendSelection, int allowOverlap);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_select_object(OcctEngineSafeHandle handle, long objectId, int appendSelection);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_selection_mode(OcctEngineSafeHandle handle, int selectionMode);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_clear_selection(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_start_rotation(OcctEngineSafeHandle handle, int x, int y);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_rotation(OcctEngineSafeHandle handle, int x, int y);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_pan(OcctEngineSafeHandle handle, int deltaX, int deltaY);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_zoom(OcctEngineSafeHandle handle, double factor);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_get_camera(OcctEngineSafeHandle handle, out OcctCameraState result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_camera(OcctEngineSafeHandle handle, in OcctCameraState state);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern double occt_get_view_scale(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_view_scale(OcctEngineSafeHandle handle, double scale);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_antialiasing(OcctEngineSafeHandle handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_gradient_background(OcctEngineSafeHandle handle, double r1, double g1, double b1, double r2, double g2, double b2, int fillMethod);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_show_all(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_hide_all(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_redisplay_object(OcctEngineSafeHandle handle, long objectId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_highlight_object(OcctEngineSafeHandle handle, long objectId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_unhighlight_object(OcctEngineSafeHandle handle, long objectId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_copy_selected_subshape_at(OcctEngineSafeHandle handle, int index);
}
