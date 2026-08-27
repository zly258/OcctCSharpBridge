using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ViewerGeometryCreationNativeMethods
{
    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_vertex_create(OcctEngineSafeHandle handle, OcctPoint3d point, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_line_create(OcctEngineSafeHandle handle, OcctPoint3d start, OcctPoint3d end, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_polyline_create(OcctEngineSafeHandle handle, IntPtr points, int count, int closed, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_triangulated_mesh_create(OcctEngineSafeHandle handle, IntPtr vertices, int vertexCount, IntPtr triangleIndices, int triangleIndexCount, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_circle_create(OcctEngineSafeHandle handle, OcctPoint3d center, OcctVector3d normal, double radius, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_arc_three_points_create(OcctEngineSafeHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_arc_center_create(OcctEngineSafeHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_ellipse_create(OcctEngineSafeHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_bezier_create(OcctEngineSafeHandle handle, IntPtr poles, int count, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_bspline_interpolated_create(OcctEngineSafeHandle handle, IntPtr points, int count, int periodic, double tolerance, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_regular_polygon_create(OcctEngineSafeHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_rectangle_wire_create(OcctEngineSafeHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_face_from_wire_create(OcctEngineSafeHandle handle, long wireId, int onlyPlane, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_plane_face_create(OcctEngineSafeHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_box_create(OcctEngineSafeHandle handle, double x, double y, double z, double dx, double dy, double dz, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_cylinder_create(OcctEngineSafeHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_sphere_create(OcctEngineSafeHandle handle, OcctPoint3d center, double radius, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_cone_create(OcctEngineSafeHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_torus_create(OcctEngineSafeHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_wedge_create(OcctEngineSafeHandle handle, double dx, double dy, double dz, double ltx, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_compound_create(OcctEngineSafeHandle handle, IntPtr shapeIds, int count, int hideInputs, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_wire_create(OcctEngineSafeHandle handle, IntPtr edgeIds, int count, int hideInputs, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_sew(OcctEngineSafeHandle handle, IntPtr shapeIds, int count, double tolerance, int hideInputs, out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_solid_from_shell_create(OcctEngineSafeHandle handle, long shellId, int hideInput, out long result);
}
