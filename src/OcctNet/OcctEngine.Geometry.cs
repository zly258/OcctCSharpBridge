namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape MakeVertex(OcctPoint3d point) { EnsureInitialized(); return CheckShape(NativeMethods.occt_make_vertex(_handle, point)); }
    public OcctShape MakeLine(OcctPoint3d start, OcctPoint3d end) { EnsureInitialized(); return CheckShape(NativeMethods.occt_make_line(_handle, start, end)); }

    public OcctShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed = false)
    {
        ArgumentNullException.ThrowIfNull(points);
        var array = points.ToArray();
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_polyline(_handle, array, array.Length, closed ? 1 : 0));
    }

    public OcctShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius) { EnsureInitialized(); return CheckShape(NativeMethods.occt_make_circle(_handle, center, normal, radius)); }
    public OcctShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end) { EnsureInitialized(); return CheckShape(NativeMethods.occt_make_arc_three_points(_handle, start, middle, end)); }
    public OcctShape MakeArc(OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees) { EnsureInitialized(); return CheckShape(NativeMethods.occt_make_arc_center(_handle, center, normal, xDirection, radius, startAngleDegrees, endAngleDegrees)); }
    public OcctShape MakeRegularPolygon(double radius, int sideCount, bool makeFace = false, OcctPoint3d? center = null, OcctVector3d? normal = null, OcctVector3d? xDirection = null) { EnsureInitialized(); return CheckShape(NativeMethods.occt_make_regular_polygon(_handle, center ?? OcctPoint3d.Origin, normal ?? OcctVector3d.UnitZ, xDirection ?? OcctVector3d.UnitX, radius, sideCount, makeFace ? 1 : 0)); }
    public OcctShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius) { EnsureInitialized(); return CheckShape(NativeMethods.occt_make_ellipse(_handle, center, normal, majorRadius, minorRadius)); }

    public OcctShape MakeBezier(IEnumerable<OcctPoint3d> poles)
    {
        ArgumentNullException.ThrowIfNull(poles);
        var array = poles.ToArray();
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_bezier(_handle, array, array.Length));
    }

    public OcctShape MakeInterpolatedBSpline(IEnumerable<OcctPoint3d> points, bool periodic = false, double tolerance = 1e-7)
    {
        ArgumentNullException.ThrowIfNull(points);
        var array = points.ToArray();
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_bspline_interpolated(_handle, array, array.Length, periodic ? 1 : 0, tolerance));
    }

    public OcctShape MakeRectangleWire(double width, double height, OcctPoint3d? origin = null, OcctVector3d? xDirection = null, OcctVector3d? normal = null)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_rectangle_wire(_handle, origin ?? OcctPoint3d.Origin, xDirection ?? OcctVector3d.UnitX, normal ?? OcctVector3d.UnitZ, width, height));
    }

    public OcctShape MakeFace(OcctShape wire, bool onlyPlane = true) { EnsureInitialized(); return CheckShape(NativeMethods.occt_make_face_from_wire(_handle, wire.Id, onlyPlane ? 1 : 0)); }

    public OcctShape MakePlaneFace(double width, double height, OcctPoint3d? origin = null, OcctVector3d? xDirection = null, OcctVector3d? normal = null)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_plane_face(_handle, origin ?? OcctPoint3d.Origin, xDirection ?? OcctVector3d.UnitX, normal ?? OcctVector3d.UnitZ, width, height));
    }

    public OcctShape MakeBox(double dx, double dy, double dz, double x = 0, double y = 0, double z = 0)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_box(_handle, x, y, z, dx, dy, dz));
    }

    public OcctShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_cylinder(_handle, origin, axis, radius, height));
    }

    public OcctShape MakeCylinder(double radius, double height, double x = 0, double y = 0, double z = 0) => MakeCylinder(new OcctPoint3d(x, y, z), OcctVector3d.UnitZ, radius, height);

    public OcctShape MakeSphere(double radius, double x = 0, double y = 0, double z = 0)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_sphere(_handle, new OcctPoint3d(x, y, z), radius));
    }

    public OcctShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_cone(_handle, origin, axis, radius1, radius2, height));
    }

    public OcctShape MakeCone(double radius1, double radius2, double height, double x = 0, double y = 0, double z = 0) => MakeCone(new OcctPoint3d(x, y, z), OcctVector3d.UnitZ, radius1, radius2, height);

    public OcctShape MakeTorus(double majorRadius, double minorRadius, OcctPoint3d? center = null, OcctVector3d? axis = null)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_torus(_handle, center ?? OcctPoint3d.Origin, axis ?? OcctVector3d.UnitZ, majorRadius, minorRadius));
    }

    public OcctShape MakeWedge(double dx, double dy, double dz, double ltx)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_wedge(_handle, dx, dy, dz, ltx));
    }

    public OcctShape MakeCompound(IEnumerable<OcctShape> shapes, bool hideInputs = false)
    {
        var ids = ShapeIds(shapes);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_compound(_handle, ids, ids.Length, hideInputs ? 1 : 0));
    }

    public OcctShape MakeWire(IEnumerable<OcctShape> edges, bool hideInputs = false)
    {
        var ids = ShapeIds(edges);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_wire(_handle, ids, ids.Length, hideInputs ? 1 : 0));
    }

    public OcctShape Sew(IEnumerable<OcctShape> shapes, double tolerance = 1e-6, bool hideInputs = false)
    {
        var ids = ShapeIds(shapes);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_sew_shapes(_handle, ids, ids.Length, tolerance, hideInputs ? 1 : 0));
    }

    public OcctShape MakeSolidFromShell(OcctShape shell, bool hideInput = false)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_solid_from_shell(_handle, shell.Id, hideInput ? 1 : 0));
    }

    private static long[] ShapeIds(IEnumerable<OcctShape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        return shapes.Select(value => value.Id).ToArray();
    }
}
