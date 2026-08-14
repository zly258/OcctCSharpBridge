namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_add_point(
        OcctEngineSafeHandle handle,
        OcctPoint3d position,
        int marker,
        double scale,
        double r,
        double g,
        double b);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_point_position(
        OcctEngineSafeHandle handle,
        long pointId,
        OcctPoint3d position);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_point_style(
        OcctEngineSafeHandle handle,
        long pointId,
        int marker,
        double scale,
        double r,
        double g,
        double b);
}
