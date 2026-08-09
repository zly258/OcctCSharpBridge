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
}
