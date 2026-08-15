using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_vertex(OcctModelingSafeHandle handle, OcctPoint3d point, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_line(OcctModelingSafeHandle handle, OcctPoint3d start, OcctPoint3d end, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_polyline(OcctModelingSafeHandle handle, [In] OcctPoint3d[] points, int count, int closed, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_circle(OcctModelingSafeHandle handle, OcctPoint3d center, OcctVector3d normal, double radius, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_arc_three_points(OcctModelingSafeHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_arc_center(OcctModelingSafeHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_ellipse(OcctModelingSafeHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_bezier(OcctModelingSafeHandle handle, [In] OcctPoint3d[] poles, int count, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_bspline_interpolated(OcctModelingSafeHandle handle, [In] OcctPoint3d[] points, int count, int periodic, double tolerance, out long result);
}
