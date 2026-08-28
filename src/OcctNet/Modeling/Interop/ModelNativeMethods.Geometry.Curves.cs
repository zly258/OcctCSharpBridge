using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_helix_create(OcctModelingSafeHandle handle, OcctPoint3d origin, OcctVector3d axis, OcctVector3d xDirection, double radius, double pitch, double turns, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_vertex_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d point,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_line_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d start,
        OcctPoint3d end,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_polyline_create(
        OcctModelingSafeHandle handle,
        [In] OcctPoint3d[] points,
        int count,
        int closed,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_circle_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        double radius,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_arc_three_points_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d start,
        OcctPoint3d middle,
        OcctPoint3d end,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_arc_center_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        double startAngleDegrees,
        double endAngleDegrees,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_ellipse_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        double majorRadius,
        double minorRadius,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_bezier_create(
        OcctModelingSafeHandle handle,
        [In] OcctPoint3d[] poles,
        int count,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_bspline_interpolated_create(
        OcctModelingSafeHandle handle,
        [In] OcctPoint3d[] points,
        int count,
        int periodic,
        double tolerance,
        out long result);
}
