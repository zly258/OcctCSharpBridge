using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_viewport_state(OcctEngineSafeHandle handle, out OcctViewportState result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_reset_view(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_reset_view_orientation(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_reset_view_mapping(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_fit_selected(OcctEngineSafeHandle handle, double margin);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_scene_gravity_point(OcctEngineSafeHandle handle, out OcctPoint3d result);
}
