using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_vertex(IntPtr handle, OcctPoint3d point);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_line(IntPtr handle, OcctPoint3d start, OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_polyline(IntPtr handle, [In] OcctPoint3d[] points, int count, int closed);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_circle(IntPtr handle, OcctPoint3d center, OcctVector3d normal, double radius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_arc_three_points(IntPtr handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_arc_center(IntPtr handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_regular_polygon(IntPtr handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_ellipse(IntPtr handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_bezier(IntPtr handle, [In] OcctPoint3d[] poles, int count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_bspline_interpolated(IntPtr handle, [In] OcctPoint3d[] points, int count, int periodic, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_rectangle_wire(IntPtr handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_plane_face(IntPtr handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_face_from_wire(IntPtr handle, long wireId, int onlyPlane);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_box(IntPtr handle, double x, double y, double z, double dx, double dy, double dz);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_cylinder(IntPtr handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_cone(IntPtr handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_sphere(IntPtr handle, OcctPoint3d center, double radius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_torus(IntPtr handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_wedge(IntPtr handle, double dx, double dy, double dz, double ltx);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_compound(IntPtr handle, [In] long[] shapeIds, int count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_wire(IntPtr handle, [In] long[] edgeIds, int count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_sew(IntPtr handle, [In] long[] shapeIds, int count, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_make_solid_from_shell(IntPtr handle, long shellId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_translate(IntPtr handle, long shapeId, OcctVector3d vector);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_rotate(IntPtr handle, long shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_scale(IntPtr handle, long shapeId, OcctPoint3d center, double factor);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_model_mirror_plane(IntPtr handle, long shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal);
}
