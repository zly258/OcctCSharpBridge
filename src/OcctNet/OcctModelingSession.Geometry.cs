namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape MakeVertex(OcctPoint3d point)
    {
        OcctGuard.Finite(point, nameof(point));
        return CheckShape(ModelNativeMethods.occt_model_make_vertex(NativeHandle, point));
    }

    public OcctModelShape MakeLine(OcctPoint3d start, OcctPoint3d end)
    {
        OcctGuard.Finite(start, nameof(start));
        OcctGuard.Finite(end, nameof(end));
        if ((end - start).LengthSquared <= 1e-30)
            throw new ArgumentException("Line endpoints must be distinct.", nameof(end));
        return CheckShape(ModelNativeMethods.occt_model_make_line(NativeHandle, start, end));
    }

    public OcctModelShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed = false)
    {
        var array = RequiredArray(points, nameof(points));
        OcctGuard.AtLeast(array.Length, closed ? 3 : 2, nameof(points));
        foreach (var point in array) OcctGuard.Finite(point, nameof(points));
        return CheckShape(ModelNativeMethods.occt_model_make_polyline(NativeHandle, array, array.Length, closed ? 1 : 0));
    }

    public OcctModelShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.NonZero(normal, nameof(normal));
        OcctGuard.Positive(radius, nameof(radius));
        return CheckShape(ModelNativeMethods.occt_model_make_circle(NativeHandle, center, normal, radius));
    }

    public OcctModelShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
    {
        OcctGuard.Finite(start, nameof(start));
        OcctGuard.Finite(middle, nameof(middle));
        OcctGuard.Finite(end, nameof(end));
        return CheckShape(ModelNativeMethods.occt_model_make_arc_three_points(NativeHandle, start, middle, end));
    }

    public OcctModelShape MakeArc(
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        double startAngleDegrees,
        double endAngleDegrees)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.NonZero(normal, nameof(normal));
        OcctGuard.NonZero(xDirection, nameof(xDirection));
        OcctGuard.Positive(radius, nameof(radius));
        OcctGuard.Finite(startAngleDegrees, nameof(startAngleDegrees));
        OcctGuard.Finite(endAngleDegrees, nameof(endAngleDegrees));
        return CheckShape(ModelNativeMethods.occt_model_make_arc_center(
            NativeHandle, center, normal, xDirection, radius, startAngleDegrees, endAngleDegrees));
    }

    public OcctModelShape MakeRegularPolygon(
        double radius,
        int sideCount,
        bool makeFace = false,
        OcctPoint3d? center = null,
        OcctVector3d? normal = null,
        OcctVector3d? xDirection = null)
    {
        OcctGuard.Positive(radius, nameof(radius));
        OcctGuard.AtLeast(sideCount, 3, nameof(sideCount));
        var actualCenter = center ?? OcctPoint3d.Origin;
        var actualNormal = normal ?? OcctVector3d.UnitZ;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        OcctGuard.Finite(actualCenter, nameof(center));
        OcctGuard.NonZero(actualNormal, nameof(normal));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        return CheckShape(ModelNativeMethods.occt_model_make_regular_polygon(
            NativeHandle, actualCenter, actualNormal, actualXDirection, radius, sideCount, makeFace ? 1 : 0));
    }

    public OcctModelShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.NonZero(normal, nameof(normal));
        OcctGuard.Positive(majorRadius, nameof(majorRadius));
        OcctGuard.Positive(minorRadius, nameof(minorRadius));
        if (minorRadius > majorRadius)
            throw new ArgumentException("minorRadius must not exceed majorRadius.", nameof(minorRadius));
        return CheckShape(ModelNativeMethods.occt_model_make_ellipse(NativeHandle, center, normal, majorRadius, minorRadius));
    }

    public OcctModelShape MakeBezier(IEnumerable<OcctPoint3d> poles)
    {
        var array = RequiredArray(poles, nameof(poles));
        OcctGuard.AtLeast(array.Length, 2, nameof(poles));
        foreach (var point in array) OcctGuard.Finite(point, nameof(poles));
        return CheckShape(ModelNativeMethods.occt_model_make_bezier(NativeHandle, array, array.Length));
    }

    public OcctModelShape MakeInterpolatedBSpline(IEnumerable<OcctPoint3d> points, bool periodic = false, double tolerance = 1e-7)
    {
        var array = RequiredArray(points, nameof(points));
        OcctGuard.AtLeast(array.Length, periodic ? 3 : 2, nameof(points));
        foreach (var point in array) OcctGuard.Finite(point, nameof(points));
        OcctGuard.Positive(tolerance, nameof(tolerance));
        return CheckShape(ModelNativeMethods.occt_model_make_bspline_interpolated(NativeHandle, array, array.Length, periodic ? 1 : 0, tolerance));
    }

    public OcctModelShape MakeRectangleWire(
        double width,
        double height,
        OcctPoint3d? origin = null,
        OcctVector3d? xDirection = null,
        OcctVector3d? normal = null)
    {
        OcctGuard.Positive(width, nameof(width));
        OcctGuard.Positive(height, nameof(height));
        var actualOrigin = origin ?? OcctPoint3d.Origin;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        var actualNormal = normal ?? OcctVector3d.UnitZ;
        OcctGuard.Finite(actualOrigin, nameof(origin));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        OcctGuard.NonZero(actualNormal, nameof(normal));
        return CheckShape(ModelNativeMethods.occt_model_make_rectangle_wire(
            NativeHandle, actualOrigin, actualXDirection, actualNormal, width, height));
    }

    public OcctModelShape MakePlaneFace(
        double width,
        double height,
        OcctPoint3d? origin = null,
        OcctVector3d? xDirection = null,
        OcctVector3d? normal = null)
    {
        OcctGuard.Positive(width, nameof(width));
        OcctGuard.Positive(height, nameof(height));
        var actualOrigin = origin ?? OcctPoint3d.Origin;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        var actualNormal = normal ?? OcctVector3d.UnitZ;
        OcctGuard.Finite(actualOrigin, nameof(origin));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        OcctGuard.NonZero(actualNormal, nameof(normal));
        return CheckShape(ModelNativeMethods.occt_model_make_plane_face(
            NativeHandle, actualOrigin, actualXDirection, actualNormal, width, height));
    }

    public OcctModelShape MakeFace(OcctModelShape wire, bool onlyPlane = true)
    {
        EnsureShape(wire);
        return CheckShape(ModelNativeMethods.occt_model_make_face_from_wire(_handle, wire.Id, onlyPlane ? 1 : 0));
    }

    public OcctModelShape MakeBox(double dx, double dy, double dz, double x = 0, double y = 0, double z = 0)
    {
        OcctGuard.Positive(dx, nameof(dx));
        OcctGuard.Positive(dy, nameof(dy));
        OcctGuard.Positive(dz, nameof(dz));
        OcctGuard.Finite(x, nameof(x));
        OcctGuard.Finite(y, nameof(y));
        OcctGuard.Finite(z, nameof(z));
        return CheckShape(ModelNativeMethods.occt_model_make_box(NativeHandle, x, y, z, dx, dy, dz));
    }

    public OcctModelShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
    {
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.Positive(radius, nameof(radius));
        OcctGuard.Positive(height, nameof(height));
        return CheckShape(ModelNativeMethods.occt_model_make_cylinder(NativeHandle, origin, axis, radius, height));
    }

    public OcctModelShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
    {
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.NonNegative(radius1, nameof(radius1));
        OcctGuard.NonNegative(radius2, nameof(radius2));
        if (radius1 == 0 && radius2 == 0)
            throw new ArgumentException("At least one cone radius must be greater than zero.", nameof(radius1));
        OcctGuard.Positive(height, nameof(height));
        return CheckShape(ModelNativeMethods.occt_model_make_cone(NativeHandle, origin, axis, radius1, radius2, height));
    }

    public OcctModelShape MakeSphere(OcctPoint3d center, double radius)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.Positive(radius, nameof(radius));
        return CheckShape(ModelNativeMethods.occt_model_make_sphere(NativeHandle, center, radius));
    }

    public OcctModelShape MakeTorus(OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.Positive(majorRadius, nameof(majorRadius));
        OcctGuard.Positive(minorRadius, nameof(minorRadius));
        return CheckShape(ModelNativeMethods.occt_model_make_torus(NativeHandle, center, axis, majorRadius, minorRadius));
    }

    public OcctModelShape MakeWedge(double dx, double dy, double dz, double ltx)
    {
        OcctGuard.Positive(dx, nameof(dx));
        OcctGuard.Positive(dy, nameof(dy));
        OcctGuard.Positive(dz, nameof(dz));
        OcctGuard.Finite(ltx, nameof(ltx));
        return CheckShape(ModelNativeMethods.occt_model_make_wedge(NativeHandle, dx, dy, dz, ltx));
    }

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
        OcctGuard.Positive(tolerance, nameof(tolerance));
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
        OcctGuard.Finite(vector, nameof(vector));
        return CheckShape(ModelNativeMethods.occt_model_translate(_handle, shape.Id, vector));
    }

    public OcctModelShape Rotate(OcctModelShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees)
    {
        EnsureShape(shape);
        OcctGuard.Finite(axisPoint, nameof(axisPoint));
        OcctGuard.NonZero(axisDirection, nameof(axisDirection));
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        return CheckShape(ModelNativeMethods.occt_model_rotate(_handle, shape.Id, axisPoint, axisDirection, angleDegrees));
    }

    public OcctModelShape Scale(OcctModelShape shape, OcctPoint3d center, double factor)
    {
        EnsureShape(shape);
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.Positive(factor, nameof(factor));
        return CheckShape(ModelNativeMethods.occt_model_scale(_handle, shape.Id, center, factor));
    }

    public OcctModelShape MirrorPlane(OcctModelShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal)
    {
        EnsureShape(shape);
        OcctGuard.Finite(planePoint, nameof(planePoint));
        OcctGuard.NonZero(planeNormal, nameof(planeNormal));
        return CheckShape(ModelNativeMethods.occt_model_mirror_plane(_handle, shape.Id, planePoint, planeNormal));
    }
}
