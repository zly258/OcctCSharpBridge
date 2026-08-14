namespace OcctNet;

internal static partial class NativeMethods
{
    // Temporary legacy object declarations. Object presentation is the last domain
    // still hosted here; delete this file after the object ABI5 migration completes.
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
}
