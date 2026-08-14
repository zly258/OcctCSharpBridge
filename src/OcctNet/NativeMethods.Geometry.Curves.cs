namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_vertex(OcctEngineSafeHandle handle, OcctPoint3d point);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_line(OcctEngineSafeHandle handle, OcctPoint3d start, OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_polyline(OcctEngineSafeHandle handle, [In] OcctPoint3d[] points, int count, int closed);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_circle(OcctEngineSafeHandle handle, OcctPoint3d center, OcctVector3d normal, double radius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_arc_three_points(OcctEngineSafeHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_arc_center(OcctEngineSafeHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_ellipse(OcctEngineSafeHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_bezier(OcctEngineSafeHandle handle, [In] OcctPoint3d[] poles, int count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_bspline_interpolated(OcctEngineSafeHandle handle, [In] OcctPoint3d[] points, int count, int periodic, double tolerance);
}
