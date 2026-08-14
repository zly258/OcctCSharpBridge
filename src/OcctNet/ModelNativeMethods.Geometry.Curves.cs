using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_vertex(OcctModelingSafeHandle handle, OcctPoint3d point);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_line(OcctModelingSafeHandle handle, OcctPoint3d start, OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_polyline(OcctModelingSafeHandle handle, [In] OcctPoint3d[] points, int count, int closed);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_circle(OcctModelingSafeHandle handle, OcctPoint3d center, OcctVector3d normal, double radius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_arc_three_points(OcctModelingSafeHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_arc_center(OcctModelingSafeHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_ellipse(OcctModelingSafeHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_bezier(OcctModelingSafeHandle handle, [In] OcctPoint3d[] poles, int count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_bspline_interpolated(OcctModelingSafeHandle handle, [In] OcctPoint3d[] points, int count, int periodic, double tolerance);
}
