namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape MakeVertex(OcctPoint3d point)
    {
        OcctGuard.Finite(point, nameof(point));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_vertex(_handle, point));
    }

    public OcctShape MakeLine(OcctPoint3d start, OcctPoint3d end)
    {
        OcctGuard.Finite(start, nameof(start));
        OcctGuard.Finite(end, nameof(end));
        if ((end - start).LengthSquared <= 1e-30)
            throw new ArgumentException("Line endpoints must be distinct.", nameof(end));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_line(_handle, start, end));
    }

    public OcctShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed = false)
    {
        ArgumentNullException.ThrowIfNull(points);
        var array = points.ToArray();
        OcctGuard.AtLeast(array.Length, closed ? 3 : 2, nameof(points));
        foreach (var point in array) OcctGuard.Finite(point, nameof(points));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_polyline(_handle, array, array.Length, closed ? 1 : 0));
    }

    public OcctShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.NonZero(normal, nameof(normal));
        OcctGuard.Positive(radius, nameof(radius));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_circle(_handle, center, normal, radius));
    }

    public OcctShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
    {
        OcctGuard.Finite(start, nameof(start));
        OcctGuard.Finite(middle, nameof(middle));
        OcctGuard.Finite(end, nameof(end));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_arc_three_points(_handle, start, middle, end));
    }

    public OcctShape MakeArc(
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
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_arc_center(
            _handle, center, normal, xDirection, radius, startAngleDegrees, endAngleDegrees));
    }

    public OcctShape MakeRegularPolygon(
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
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_regular_polygon(
            _handle, actualCenter, actualNormal, actualXDirection, radius, sideCount, makeFace ? 1 : 0));
    }

    public OcctShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.NonZero(normal, nameof(normal));
        OcctGuard.Positive(majorRadius, nameof(majorRadius));
        OcctGuard.Positive(minorRadius, nameof(minorRadius));
        if (minorRadius > majorRadius)
            throw new ArgumentException("minorRadius must not exceed majorRadius.", nameof(minorRadius));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_ellipse(_handle, center, normal, majorRadius, minorRadius));
    }

    public OcctShape MakeBezier(IEnumerable<OcctPoint3d> poles)
    {
        ArgumentNullException.ThrowIfNull(poles);
        var array = poles.ToArray();
        OcctGuard.AtLeast(array.Length, 2, nameof(poles));
        foreach (var point in array) OcctGuard.Finite(point, nameof(poles));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_bezier(_handle, array, array.Length));
    }

    public OcctShape MakeInterpolatedBSpline(IEnumerable<OcctPoint3d> points, bool periodic = false, double tolerance = 1e-7)
    {
        ArgumentNullException.ThrowIfNull(points);
        var array = points.ToArray();
        OcctGuard.AtLeast(array.Length, periodic ? 3 : 2, nameof(points));
        foreach (var point in array) OcctGuard.Finite(point, nameof(points));
        OcctGuard.Positive(tolerance, nameof(tolerance));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_bspline_interpolated(_handle, array, array.Length, periodic ? 1 : 0, tolerance));
    }

    public OcctShape MakeRectangleWire(
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
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_rectangle_wire(
            _handle, actualOrigin, actualXDirection, actualNormal, width, height));
    }

    public OcctShape MakeFace(OcctShape wire, bool onlyPlane = true)
    {
        EnsureShape(wire);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_face_from_wire(_handle, wire.Id, onlyPlane ? 1 : 0));
    }

    public OcctShape MakePlaneFace(
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
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_plane_face(
            _handle, actualOrigin, actualXDirection, actualNormal, width, height));
    }

    public OcctShape MakeBox(double dx, double dy, double dz, double x = 0, double y = 0, double z = 0)
    {
        OcctGuard.Positive(dx, nameof(dx));
        OcctGuard.Positive(dy, nameof(dy));
        OcctGuard.Positive(dz, nameof(dz));
        OcctGuard.Finite(x, nameof(x));
        OcctGuard.Finite(y, nameof(y));
        OcctGuard.Finite(z, nameof(z));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_box(_handle, x, y, z, dx, dy, dz));
    }

    public OcctShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
    {
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.Positive(radius, nameof(radius));
        OcctGuard.Positive(height, nameof(height));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_cylinder(_handle, origin, axis, radius, height));
    }

    public OcctShape MakeCylinder(double radius, double height, double x = 0, double y = 0, double z = 0) =>
        MakeCylinder(new OcctPoint3d(x, y, z), OcctVector3d.UnitZ, radius, height);

    public OcctShape MakeSphere(double radius, double x = 0, double y = 0, double z = 0)
    {
        OcctGuard.Positive(radius, nameof(radius));
        var center = new OcctPoint3d(x, y, z);
        OcctGuard.Finite(center, nameof(center));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_sphere(_handle, center, radius));
    }

    public OcctShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
    {
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.NonNegative(radius1, nameof(radius1));
        OcctGuard.NonNegative(radius2, nameof(radius2));
        if (radius1 == 0 && radius2 == 0)
            throw new ArgumentException("At least one cone radius must be greater than zero.", nameof(radius1));
        OcctGuard.Positive(height, nameof(height));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_cone(_handle, origin, axis, radius1, radius2, height));
    }

    public OcctShape MakeCone(double radius1, double radius2, double height, double x = 0, double y = 0, double z = 0) =>
        MakeCone(new OcctPoint3d(x, y, z), OcctVector3d.UnitZ, radius1, radius2, height);

    public OcctShape MakeTorus(double majorRadius, double minorRadius, OcctPoint3d? center = null, OcctVector3d? axis = null)
    {
        OcctGuard.Positive(majorRadius, nameof(majorRadius));
        OcctGuard.Positive(minorRadius, nameof(minorRadius));
        var actualCenter = center ?? OcctPoint3d.Origin;
        var actualAxis = axis ?? OcctVector3d.UnitZ;
        OcctGuard.Finite(actualCenter, nameof(center));
        OcctGuard.NonZero(actualAxis, nameof(axis));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_torus(_handle, actualCenter, actualAxis, majorRadius, minorRadius));
    }

    public OcctShape MakeWedge(double dx, double dy, double dz, double ltx)
    {
        OcctGuard.Positive(dx, nameof(dx));
        OcctGuard.Positive(dy, nameof(dy));
        OcctGuard.Positive(dz, nameof(dz));
        OcctGuard.Finite(ltx, nameof(ltx));
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
        OcctGuard.Positive(tolerance, nameof(tolerance));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_sew_shapes(_handle, ids, ids.Length, tolerance, hideInputs ? 1 : 0));
    }

    public OcctShape MakeSolidFromShell(OcctShape shell, bool hideInput = false)
    {
        EnsureShape(shell);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_solid_from_shell(_handle, shell.Id, hideInput ? 1 : 0));
    }

    private long[] ShapeIds(IEnumerable<OcctShape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        var array = shapes.ToArray();
        if (array.Length == 0) throw new ArgumentException("Collection must not be empty.", nameof(shapes));
        foreach (var shape in array) EnsureShape(shape);
        return array.Select(value => value.Id).Distinct().ToArray();
    }
}
