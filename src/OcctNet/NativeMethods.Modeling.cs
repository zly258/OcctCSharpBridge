namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_vertex(IntPtr handle, OcctPoint3d point);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_line(IntPtr handle, OcctPoint3d start, OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_polyline(IntPtr handle, [In] OcctPoint3d[] points, int count, int closed);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_circle(IntPtr handle, OcctPoint3d center, OcctVector3d normal, double radius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_arc_three_points(IntPtr handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_ellipse(IntPtr handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_bezier(IntPtr handle, [In] OcctPoint3d[] poles, int count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_bspline_interpolated(IntPtr handle, [In] OcctPoint3d[] points, int count, int periodic, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_rectangle_wire(IntPtr handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_face_from_wire(IntPtr handle, long wireId, int onlyPlane);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_plane_face(IntPtr handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_arc_center(IntPtr handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_regular_polygon(IntPtr handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_box(IntPtr handle, double x, double y, double z, double dx, double dy, double dz);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_cylinder(IntPtr handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_sphere(IntPtr handle, OcctPoint3d center, double radius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_cone(IntPtr handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_torus(IntPtr handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_wedge(IntPtr handle, double dx, double dy, double dz, double ltx);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_compound(IntPtr handle, [In] long[] shapeIds, int count, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_wire(IntPtr handle, [In] long[] edgeIds, int count, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_sew_shapes(IntPtr handle, [In] long[] shapeIds, int count, double tolerance, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_solid_from_shell(IntPtr handle, long shellId, int hideInput);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_boolean(IntPtr handle, int operation, long leftId, long rightId, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_extrude(IntPtr handle, long profileId, OcctVector3d vector, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_revolve(IntPtr handle, long profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_sweep(IntPtr handle, long spineWireId, long profileId, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_loft(IntPtr handle, [In] long[] wireIds, int count, int makeSolid, int ruled, double tolerance, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_fillet_all_edges(IntPtr handle, long shapeId, double radius, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_chamfer_all_edges(IntPtr handle, long shapeId, double distance, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_offset_shape(IntPtr handle, long shapeId, double offset, double tolerance, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_thick_solid(IntPtr handle, long solidId, int faceIndexToRemove, double thickness, double tolerance, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_fillet_edges(IntPtr handle, long shapeId, [In] int[] edgeIndices, int count, double radius, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_chamfer_edges(IntPtr handle, long shapeId, [In] int[] edgeIndices, int count, double distance, int hideInput);
}
