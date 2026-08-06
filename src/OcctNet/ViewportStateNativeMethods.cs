using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_get_viewport_state(IntPtr handle, out OcctViewportState result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_reset_view(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_reset_view_orientation(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_reset_view_mapping(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_fit_selected(IntPtr handle, double margin);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_get_scene_gravity_point(IntPtr handle, out OcctPoint3d result);
}
