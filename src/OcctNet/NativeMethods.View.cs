namespace OcctNet;

internal static partial class NativeMethods
{
    // Temporary legacy surface/selection/object declarations. These remaining domains
    // are being migrated to ABI5 and this file will be deleted when that work completes.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_initialize(OcctEngineSafeHandle handle, IntPtr windowHandle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_resize(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_resize_surface(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_redraw(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_selection_tolerance(OcctEngineSafeHandle handle, int pixelTolerance);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_move_to(OcctEngineSafeHandle handle, int x, int y);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_select(OcctEngineSafeHandle handle, int x, int y, int appendSelection);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_select_rectangle_ex(
        OcctEngineSafeHandle handle,
        int x1,
        int y1,
        int x2,
        int y2,
        int appendSelection,
        int allowOverlap);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_select_object(OcctEngineSafeHandle handle, long objectId, int appendSelection);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_selection_mode(OcctEngineSafeHandle handle, int selectionMode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_clear_selection(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_show_all(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_hide_all(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_redisplay_object(OcctEngineSafeHandle handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_highlight_object(OcctEngineSafeHandle handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_unhighlight_object(OcctEngineSafeHandle handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_copy_selected_subshape_at(OcctEngineSafeHandle handle, int index);
}
