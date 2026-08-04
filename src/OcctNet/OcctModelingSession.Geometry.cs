namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape MakeVertex(OcctPoint3d point) => CheckShape(ModelNativeMethods.occt_model_make_vertex(NativeHandle, point));
    public OcctModelShape MakeLine(OcctPoint3d start, OcctPoint3d end) => CheckShape(ModelNativeMethods.occt_model_make_line(NativeHandle, start, end));

    public OcctModelShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed = false)
    {
        var array = RequiredArray(points, nameof(points));
        return CheckShape(ModelNativeMethods.occt_model_make_polyline(NativeHandle, array, array.Length, closed ? 1 : 0));
    }

    public OcctModelShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius) =>
        CheckShape(ModelNativeMethods.occt_model_make_circle(NativeHandle, center, normal, radius));

    public OcctModelShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end) =>
        CheckShape(ModelNativeMethods.occt_model_make_arc_three_points(NativeHandle, start, middle, end));

    public OcctModelShape MakeArc(
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        double startAngleDegrees,
        double endAngleDegrees) =>
        CheckShape(ModelNativeMethods.occt_model_make_arc_center(
            NativeHandle, center, normal, xDirection, radius, startAngleDegrees, endAngleDegrees));

    public OcctModelShape MakeRegularPolygon(
        double radius,
        int sideCount,
        bool makeFace = false,
        OcctPoint3d? center = null,
        OcctVector3d? normal = null,
        OcctVector3d? xDirection = null) =>
        CheckShape(ModelNativeMethods.occt_model_make_regular_polygon(
            NativeHandle,
            center ?? OcctPoint3d.Origin,
            normal ?? OcctVector3d.UnitZ,
            xDirection ?? OcctVector3d.UnitX,
            radius,
            sideCount,
            makeFace ? 1 : 0));

    public OcctModelShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius) =>
        CheckShape(ModelNativeMethods.occt_model_make_ellipse(NativeHandle, center, normal, majorRadius, minorRadius));

    public OcctModelShape MakeBezier(IEnumerable<OcctPoint3d> poles)
    {
        var array = RequiredArray(poles, nameof(poles));
        return CheckShape(ModelNativeMethods.occt_model_make_bezier(NativeHandle, array, array.Length));
    }

    public OcctModelShape MakeInterpolatedBSpline(IEnumerable<OcctPoint3d> points, bool periodic = false, double tolerance = 1e-7)
    {
        var array = RequiredArray(points, nameof(points));
        return CheckShape(ModelNativeMethods.occt_model_make_bspline_interpolated(NativeHandle, array, array.Length, periodic ? 1 : 0, tolerance));
    }

    public OcctModelShape MakeRectangleWire(
        double width,
        double height,
        OcctPoint3d? origin = null,
        OcctVector3d? xDirection = null,
        OcctVector3d? normal = null) =>
        CheckShape(ModelNativeMethods.occt_model_make_rectangle_wire(
            NativeHandle,
            origin ?? OcctPoint3d.Origin,
            xDirection ?? OcctVector3d.UnitX,
            normal ?? OcctVector3d.UnitZ,
            width,
            height));

    public OcctModelShape MakePlaneFace(
        double width,
        double height,
        OcctPoint3d? origin = null,
        OcctVector3d? xDirection = null,
        OcctVector3d? normal = null) =>
        CheckShape(ModelNativeMethods.occt_model_make_plane_face(
            NativeHandle,
            origin ?? OcctPoint3d.Origin,
            xDirection ?? OcctVector3d.UnitX,
            normal ?? OcctVector3d.UnitZ,
            width,
            height));

    public OcctModelShape MakeFace(OcctModelShape wire, bool onlyPlane = true)
    {
        EnsureShape(wire);
        return CheckShape(ModelNativeMethods.occt_model_make_face_from_wire(_handle, wire.Id, onlyPlane ? 1 : 0));
    }

    public OcctModelShape MakeBox(double dx, double dy, double dz, double x = 0, double y = 0, double z = 0) =>
        CheckShape(ModelNativeMethods.occt_model_make_box(NativeHandle, x, y, z, dx, dy, dz));

    public OcctModelShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height) =>
        CheckShape(ModelNativeMethods.occt_model_make_cylinder(NativeHandle, origin, axis, radius, height));

    public OcctModelShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height) =>
        CheckShape(ModelNativeMethods.occt_model_make_cone(NativeHandle, origin, axis, radius1, radius2, height));

    public OcctModelShape MakeSphere(OcctPoint3d center, double radius) =>
        CheckShape(ModelNativeMethods.occt_model_make_sphere(NativeHandle, center, radius));

    public OcctModelShape MakeTorus(OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius) =>
        CheckShape(ModelNativeMethods.occt_model_make_torus(NativeHandle, center, axis, majorRadius, minorRadius));

    public OcctModelShape MakeWedge(double dx, double dy, double dz, double ltx) =>
        CheckShape(ModelNativeMethods.occt_model_make_wedge(NativeHandle, dx, dy, dz, ltx));

    public OcctModelShape MakeCompound(IEnumerable<OcctModelShape> shapes)
    {
        var ids = ShapeIds(shapes);
        return CheckShape(ModelNativeMethods.occt_model_make_compound(NativeHandle, ids, ids.Length));
    }

    public OcctModelShape MakeWire(IEnumerable<OcctModelShape> edges)
    {
        var ids = ShapeIds(edges);
        return CheckShape(ModelNativeMethods.occt_model_make_wire(NativeHandle, ids, ids.Length));
    }

    public OcctModelShape Sew(IEnumerable<OcctModelShape> shapes, double tolerance = 1e-6)
    {
        var ids = ShapeIds(shapes);
        return CheckShape(ModelNativeMethods.occt_model_sew(NativeHandle, ids, ids.Length, tolerance));
    }

    public OcctModelShape MakeSolidFromShell(OcctModelShape shell)
    {
        EnsureShape(shell);
        return CheckShape(ModelNativeMethods.occt_model_make_solid_from_shell(_handle, shell.Id));
    }

    public OcctModelShape Translate(OcctModelShape shape, OcctVector3d vector)
    {
        EnsureShape(shape);
        return CheckShape(ModelNativeMethods.occt_model_translate(_handle, shape.Id, vector));
    }

    public OcctModelShape Rotate(OcctModelShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees)
    {
        EnsureShape(shape);
        return CheckShape(ModelNativeMethods.occt_model_rotate(_handle, shape.Id, axisPoint, axisDirection, angleDegrees));
    }

    public OcctModelShape Scale(OcctModelShape shape, OcctPoint3d center, double factor)
    {
        EnsureShape(shape);
        return CheckShape(ModelNativeMethods.occt_model_scale(_handle, shape.Id, center, factor));
    }

    public OcctModelShape MirrorPlane(OcctModelShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal)
    {
        EnsureShape(shape);
        return CheckShape(ModelNativeMethods.occt_model_mirror_plane(_handle, shape.Id, planePoint, planeNormal));
    }
}
