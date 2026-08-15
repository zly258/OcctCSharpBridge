namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape MakeVertex(OcctPoint3d point)
    {
        OcctGuard.Finite(point, nameof(point));
        var status = ModelNativeMethods.occt_model_curve_vertex_create(NativeHandle, point, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeLine(OcctPoint3d start, OcctPoint3d end)
    {
        OcctGuard.Finite(start, nameof(start));
        OcctGuard.Finite(end, nameof(end));
        if ((end - start).LengthSquared <= 1e-30)
            throw new ArgumentException("Line endpoints must be distinct.", nameof(end));
        var status = ModelNativeMethods.occt_model_curve_line_create(NativeHandle, start, end, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed = false)
    {
        var array = RequiredArray(points, nameof(points));
        OcctGuard.AtLeast(array.Length, closed ? 3 : 2, nameof(points));
        foreach (var point in array) OcctGuard.Finite(point, nameof(points));
        var status = ModelNativeMethods.occt_model_curve_polyline_create(
            NativeHandle, array, array.Length, closed ? 1 : 0, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.NonZero(normal, nameof(normal));
        OcctGuard.Positive(radius, nameof(radius));
        var status = ModelNativeMethods.occt_model_curve_circle_create(
            NativeHandle, center, normal, radius, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
    {
        OcctGuard.Finite(start, nameof(start));
        OcctGuard.Finite(middle, nameof(middle));
        OcctGuard.Finite(end, nameof(end));
        var status = ModelNativeMethods.occt_model_curve_arc_three_points_create(
            NativeHandle, start, middle, end, out var result);
        return CheckShape(status, result);
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
        var status = ModelNativeMethods.occt_model_curve_arc_center_create(
            NativeHandle,
            center,
            normal,
            xDirection,
            radius,
            startAngleDegrees,
            endAngleDegrees,
            out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.NonZero(normal, nameof(normal));
        OcctGuard.Positive(majorRadius, nameof(majorRadius));
        OcctGuard.Positive(minorRadius, nameof(minorRadius));
        if (minorRadius > majorRadius)
            throw new ArgumentException("minorRadius must not exceed majorRadius.", nameof(minorRadius));
        var status = ModelNativeMethods.occt_model_curve_ellipse_create(
            NativeHandle, center, normal, majorRadius, minorRadius, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeBezier(IEnumerable<OcctPoint3d> poles)
    {
        var array = RequiredArray(poles, nameof(poles));
        OcctGuard.AtLeast(array.Length, 2, nameof(poles));
        foreach (var point in array) OcctGuard.Finite(point, nameof(poles));
        var status = ModelNativeMethods.occt_model_curve_bezier_create(
            NativeHandle, array, array.Length, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeInterpolatedBSpline(
        IEnumerable<OcctPoint3d> points,
        bool periodic = false,
        double tolerance = 1e-7)
    {
        var array = RequiredArray(points, nameof(points));
        OcctGuard.AtLeast(array.Length, periodic ? 3 : 2, nameof(points));
        foreach (var point in array) OcctGuard.Finite(point, nameof(points));
        OcctGuard.Positive(tolerance, nameof(tolerance));
        var status = ModelNativeMethods.occt_model_curve_bspline_interpolated_create(
            NativeHandle,
            array,
            array.Length,
            periodic ? 1 : 0,
            tolerance,
            out var result);
        return CheckShape(status, result);
    }
}
