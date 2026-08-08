namespace OcctNet;

/// <summary>
/// Pure managed geometry helpers for common CAD calculations that do not require a native OCCT call.
/// Matrix helpers use row-major affine matrices and column-vector semantics.
/// </summary>
public static class OcctGeometryExtensions
{
    public static OcctPoint3d Lerp(this OcctPoint3d from, OcctPoint3d to, double amount)
    {
        OcctGuard.Finite(from, nameof(from));
        OcctGuard.Finite(to, nameof(to));
        OcctGuard.Finite(amount, nameof(amount));
        return new OcctPoint3d(
            from.X + (to.X - from.X) * amount,
            from.Y + (to.Y - from.Y) * amount,
            from.Z + (to.Z - from.Z) * amount);
    }

    public static bool AlmostEquals(this OcctPoint3d first, OcctPoint3d second, double tolerance = 1e-9)
    {
        OcctGuard.NonNegative(tolerance, nameof(tolerance));
        return first.IsFinite && second.IsFinite && first.DistanceTo(second) <= tolerance;
    }

    public static bool AlmostEquals(this OcctVector3d first, OcctVector3d second, double tolerance = 1e-9)
    {
        OcctGuard.NonNegative(tolerance, nameof(tolerance));
        return first.IsFinite && second.IsFinite && (first - second).Length <= tolerance;
    }

    public static double AngleTo(this OcctVector3d first, OcctVector3d second)
    {
        OcctGuard.NonZero(first, nameof(first));
        OcctGuard.NonZero(second, nameof(second));
        var denominator = first.Length * second.Length;
        var cosine = Math.Clamp(first.Dot(second) / denominator, -1.0, 1.0);
        return Math.Acos(cosine);
    }

    public static OcctVector3d ProjectOnto(this OcctVector3d vector, OcctVector3d axis)
    {
        OcctGuard.Finite(vector, nameof(vector));
        OcctGuard.NonZero(axis, nameof(axis));
        var scale = vector.Dot(axis) / axis.LengthSquared;
        return axis * scale;
    }

    public static OcctVector3d RejectFrom(this OcctVector3d vector, OcctVector3d axis) =>
        vector - vector.ProjectOnto(axis);

    public static bool IsFinite(this OcctBounds bounds) =>
        double.IsFinite(bounds.MinX) && double.IsFinite(bounds.MinY) && double.IsFinite(bounds.MinZ) &&
        double.IsFinite(bounds.MaxX) && double.IsFinite(bounds.MaxY) && double.IsFinite(bounds.MaxZ);

    public static bool IsValid(this OcctBounds bounds) =>
        bounds.IsFinite() &&
        bounds.MaxX >= bounds.MinX &&
        bounds.MaxY >= bounds.MinY &&
        bounds.MaxZ >= bounds.MinZ;

    public static OcctPoint3d GetMinimumPoint(this OcctBounds bounds)
    {
        EnsureValidBounds(bounds, nameof(bounds));
        return new OcctPoint3d(bounds.MinX, bounds.MinY, bounds.MinZ);
    }

    public static OcctPoint3d GetMaximumPoint(this OcctBounds bounds)
    {
        EnsureValidBounds(bounds, nameof(bounds));
        return new OcctPoint3d(bounds.MaxX, bounds.MaxY, bounds.MaxZ);
    }

    public static double GetVolume(this OcctBounds bounds)
    {
        EnsureValidBounds(bounds, nameof(bounds));
        return bounds.SizeX * bounds.SizeY * bounds.SizeZ;
    }

    public static double GetDiagonalLength(this OcctBounds bounds)
    {
        EnsureValidBounds(bounds, nameof(bounds));
        return new OcctVector3d(bounds.SizeX, bounds.SizeY, bounds.SizeZ).Length;
    }

    public static bool Contains(this OcctBounds bounds, OcctPoint3d point, double tolerance = 0)
    {
        EnsureValidBounds(bounds, nameof(bounds));
        OcctGuard.Finite(point, nameof(point));
        OcctGuard.NonNegative(tolerance, nameof(tolerance));
        return point.X >= bounds.MinX - tolerance && point.X <= bounds.MaxX + tolerance &&
               point.Y >= bounds.MinY - tolerance && point.Y <= bounds.MaxY + tolerance &&
               point.Z >= bounds.MinZ - tolerance && point.Z <= bounds.MaxZ + tolerance;
    }

    public static bool Intersects(this OcctBounds first, OcctBounds second, double tolerance = 0)
    {
        EnsureValidBounds(first, nameof(first));
        EnsureValidBounds(second, nameof(second));
        OcctGuard.NonNegative(tolerance, nameof(tolerance));
        return first.MinX <= second.MaxX + tolerance && first.MaxX + tolerance >= second.MinX &&
               first.MinY <= second.MaxY + tolerance && first.MaxY + tolerance >= second.MinY &&
               first.MinZ <= second.MaxZ + tolerance && first.MaxZ + tolerance >= second.MinZ;
    }

    public static OcctBounds Expanded(this OcctBounds bounds, double margin)
    {
        EnsureValidBounds(bounds, nameof(bounds));
        OcctGuard.NonNegative(margin, nameof(margin));
        return new OcctBounds
        {
            MinX = bounds.MinX - margin,
            MinY = bounds.MinY - margin,
            MinZ = bounds.MinZ - margin,
            MaxX = bounds.MaxX + margin,
            MaxY = bounds.MaxY + margin,
            MaxZ = bounds.MaxZ + margin
        };
    }

    public static OcctBounds Union(this OcctBounds first, OcctBounds second)
    {
        EnsureValidBounds(first, nameof(first));
        EnsureValidBounds(second, nameof(second));
        return new OcctBounds
        {
            MinX = Math.Min(first.MinX, second.MinX),
            MinY = Math.Min(first.MinY, second.MinY),
            MinZ = Math.Min(first.MinZ, second.MinZ),
            MaxX = Math.Max(first.MaxX, second.MaxX),
            MaxY = Math.Max(first.MaxY, second.MaxY),
            MaxZ = Math.Max(first.MaxZ, second.MaxZ)
        };
    }

    public static bool IsFinite(this OcctUvBounds bounds) =>
        double.IsFinite(bounds.UMin) && double.IsFinite(bounds.UMax) &&
        double.IsFinite(bounds.VMin) && double.IsFinite(bounds.VMax);

    public static bool IsValid(this OcctUvBounds bounds) =>
        bounds.IsFinite() && bounds.UMax >= bounds.UMin && bounds.VMax >= bounds.VMin;

    public static (double U, double V) GetCenter(this OcctUvBounds bounds)
    {
        EnsureValidUvBounds(bounds, nameof(bounds));
        return ((bounds.UMin + bounds.UMax) * 0.5, (bounds.VMin + bounds.VMax) * 0.5);
    }

    public static bool Contains(this OcctUvBounds bounds, double u, double v, double tolerance = 0)
    {
        EnsureValidUvBounds(bounds, nameof(bounds));
        OcctGuard.Finite(u, nameof(u));
        OcctGuard.Finite(v, nameof(v));
        OcctGuard.NonNegative(tolerance, nameof(tolerance));
        return u >= bounds.UMin - tolerance && u <= bounds.UMax + tolerance &&
               v >= bounds.VMin - tolerance && v <= bounds.VMax + tolerance;
    }

    public static bool IsFinite(this OcctDistanceResult result) =>
        double.IsFinite(result.Distance) && result.Distance >= 0 &&
        result.PointOnFirst.IsFinite && result.PointOnSecond.IsFinite;

    public static OcctVector3d GetSeparationVector(this OcctDistanceResult result) =>
        result.PointOnSecond - result.PointOnFirst;

    public static OcctPoint3d GetMidpoint(this OcctDistanceResult result) =>
        result.PointOnFirst.Lerp(result.PointOnSecond, 0.5);

    public static bool IsWithin(this OcctDistanceResult result, double tolerance)
    {
        OcctGuard.NonNegative(tolerance, nameof(tolerance));
        return result.IsFinite() && result.Distance <= tolerance;
    }

    public static bool IsAffine(this OcctModelLocation transform, double tolerance = 1e-12)
    {
        OcctGuard.NonNegative(tolerance, nameof(tolerance));
        return transform.IsFinite &&
               Math.Abs(transform.M41) <= tolerance &&
               Math.Abs(transform.M42) <= tolerance &&
               Math.Abs(transform.M43) <= tolerance &&
               Math.Abs(transform.M44 - 1.0) <= tolerance;
    }

    public static OcctPoint3d TransformPoint(this OcctModelLocation transform, OcctPoint3d point)
    {
        EnsureAffine(transform, nameof(transform));
        OcctGuard.Finite(point, nameof(point));
        return new OcctPoint3d(
            transform.M11 * point.X + transform.M12 * point.Y + transform.M13 * point.Z + transform.M14,
            transform.M21 * point.X + transform.M22 * point.Y + transform.M23 * point.Z + transform.M24,
            transform.M31 * point.X + transform.M32 * point.Y + transform.M33 * point.Z + transform.M34);
    }

    public static OcctVector3d TransformVector(this OcctModelLocation transform, OcctVector3d vector)
    {
        EnsureAffine(transform, nameof(transform));
        OcctGuard.Finite(vector, nameof(vector));
        return new OcctVector3d(
            transform.M11 * vector.X + transform.M12 * vector.Y + transform.M13 * vector.Z,
            transform.M21 * vector.X + transform.M22 * vector.Y + transform.M23 * vector.Z,
            transform.M31 * vector.X + transform.M32 * vector.Y + transform.M33 * vector.Z);
    }

    /// <summary>
    /// Returns <paramref name="left"/> × <paramref name="right"/>. With column-vector semantics,
    /// <paramref name="right"/> is applied first and <paramref name="left"/> second.
    /// </summary>
    public static OcctModelLocation Multiply(this OcctModelLocation left, OcctModelLocation right)
    {
        EnsureAffine(left, nameof(left));
        EnsureAffine(right, nameof(right));
        return new OcctModelLocation
        {
            M11 = left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31,
            M12 = left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32,
            M13 = left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33,
            M14 = left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14,
            M21 = left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31,
            M22 = left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32,
            M23 = left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33,
            M24 = left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24,
            M31 = left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31,
            M32 = left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32,
            M33 = left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33,
            M34 = left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34,
            M41 = 0,
            M42 = 0,
            M43 = 0,
            M44 = 1
        };
    }

    public static bool TryInvert(this OcctModelLocation transform, out OcctModelLocation inverse)
    {
        inverse = default;
        if (!transform.IsAffine()) return false;

        var a = transform.M11;
        var b = transform.M12;
        var c = transform.M13;
        var d = transform.M21;
        var e = transform.M22;
        var f = transform.M23;
        var g = transform.M31;
        var h = transform.M32;
        var i = transform.M33;
        var determinant = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
        if (!double.IsFinite(determinant) || Math.Abs(determinant) <= 1e-15) return false;

        var invDet = 1.0 / determinant;
        var r11 = (e * i - f * h) * invDet;
        var r12 = (c * h - b * i) * invDet;
        var r13 = (b * f - c * e) * invDet;
        var r21 = (f * g - d * i) * invDet;
        var r22 = (a * i - c * g) * invDet;
        var r23 = (c * d - a * f) * invDet;
        var r31 = (d * h - e * g) * invDet;
        var r32 = (b * g - a * h) * invDet;
        var r33 = (a * e - b * d) * invDet;

        inverse = new OcctModelLocation
        {
            M11 = r11,
            M12 = r12,
            M13 = r13,
            M14 = -(r11 * transform.M14 + r12 * transform.M24 + r13 * transform.M34),
            M21 = r21,
            M22 = r22,
            M23 = r23,
            M24 = -(r21 * transform.M14 + r22 * transform.M24 + r23 * transform.M34),
            M31 = r31,
            M32 = r32,
            M33 = r33,
            M34 = -(r31 * transform.M14 + r32 * transform.M24 + r33 * transform.M34),
            M41 = 0,
            M42 = 0,
            M43 = 0,
            M44 = 1
        };
        return true;
    }

    public static OcctModelLocation Inverted(this OcctModelLocation transform)
    {
        if (!transform.TryInvert(out var inverse))
            throw new InvalidOperationException("Transformation must be finite, affine, and invertible.");
        return inverse;
    }

    public static OcctTransform3d ToTransform3d(this OcctModelLocation transform)
    {
        EnsureAffine(transform, nameof(transform));
        return new OcctTransform3d(
            transform.M11, transform.M12, transform.M13, transform.M14,
            transform.M21, transform.M22, transform.M23, transform.M24,
            transform.M31, transform.M32, transform.M33, transform.M34);
    }

    public static OcctModelLocation ToModelLocation(this OcctTransform3d transform)
    {
        if (!transform.IsFinite)
            throw new ArgumentException("Transformation matrix must contain only finite values.", nameof(transform));
        return new OcctModelLocation
        {
            M11 = transform.M00,
            M12 = transform.M01,
            M13 = transform.M02,
            M14 = transform.M03,
            M21 = transform.M10,
            M22 = transform.M11,
            M23 = transform.M12,
            M24 = transform.M13,
            M31 = transform.M20,
            M32 = transform.M21,
            M33 = transform.M22,
            M34 = transform.M23,
            M41 = 0,
            M42 = 0,
            M43 = 0,
            M44 = 1
        };
    }

    public static OcctPoint3d TransformPoint(this OcctTransform3d transform, OcctPoint3d point) =>
        transform.ToModelLocation().TransformPoint(point);

    public static OcctVector3d TransformVector(this OcctTransform3d transform, OcctVector3d vector) =>
        transform.ToModelLocation().TransformVector(vector);

    public static OcctTransform3d Multiply(this OcctTransform3d left, OcctTransform3d right) =>
        left.ToModelLocation().Multiply(right.ToModelLocation()).ToTransform3d();

    public static OcctModelLocation CreateTranslationLocation(double x, double y, double z)
    {
        OcctGuard.Finite(x, nameof(x));
        OcctGuard.Finite(y, nameof(y));
        OcctGuard.Finite(z, nameof(z));
        var result = OcctModelLocation.Identity;
        result.M14 = x;
        result.M24 = y;
        result.M34 = z;
        return result;
    }

    public static OcctModelLocation CreateUniformScaleLocation(double scale, OcctPoint3d center = default)
    {
        OcctGuard.Finite(scale, nameof(scale));
        if (Math.Abs(scale) <= 1e-15)
            throw new ArgumentOutOfRangeException(nameof(scale), scale, "Scale must be non-zero.");
        OcctGuard.Finite(center, nameof(center));
        return new OcctModelLocation
        {
            M11 = scale,
            M22 = scale,
            M33 = scale,
            M14 = center.X * (1.0 - scale),
            M24 = center.Y * (1.0 - scale),
            M34 = center.Z * (1.0 - scale),
            M44 = 1
        };
    }

    public static OcctModelLocation CreateRotationLocation(
        OcctVector3d axis,
        double angleRadians,
        OcctPoint3d center = default)
    {
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.Finite(angleRadians, nameof(angleRadians));
        OcctGuard.Finite(center, nameof(center));

        var unit = axis.Normalized();
        var x = unit.X;
        var y = unit.Y;
        var z = unit.Z;
        var c = Math.Cos(angleRadians);
        var s = Math.Sin(angleRadians);
        var t = 1.0 - c;

        var result = new OcctModelLocation
        {
            M11 = t * x * x + c,
            M12 = t * x * y - s * z,
            M13 = t * x * z + s * y,
            M21 = t * x * y + s * z,
            M22 = t * y * y + c,
            M23 = t * y * z - s * x,
            M31 = t * x * z - s * y,
            M32 = t * y * z + s * x,
            M33 = t * z * z + c,
            M44 = 1
        };

        var rotatedCenter = new OcctPoint3d(
            result.M11 * center.X + result.M12 * center.Y + result.M13 * center.Z,
            result.M21 * center.X + result.M22 * center.Y + result.M23 * center.Z,
            result.M31 * center.X + result.M32 * center.Y + result.M33 * center.Z);
        result.M14 = center.X - rotatedCenter.X;
        result.M24 = center.Y - rotatedCenter.Y;
        result.M34 = center.Z - rotatedCenter.Z;
        return result;
    }

    public static OcctTransform3d CreateUniformScaleTransform(double scale, OcctPoint3d center = default) =>
        CreateUniformScaleLocation(scale, center).ToTransform3d();

    public static OcctTransform3d CreateRotationTransform(
        OcctVector3d axis,
        double angleRadians,
        OcctPoint3d center = default) =>
        CreateRotationLocation(axis, angleRadians, center).ToTransform3d();

    private static void EnsureValidBounds(OcctBounds bounds, string parameterName)
    {
        if (!bounds.IsValid())
            throw new ArgumentException("Bounds must be finite and have minimum coordinates not greater than maximum coordinates.", parameterName);
    }

    private static void EnsureValidUvBounds(OcctUvBounds bounds, string parameterName)
    {
        if (!bounds.IsValid())
            throw new ArgumentException("UV bounds must be finite and ordered.", parameterName);
    }

    private static void EnsureAffine(OcctModelLocation transform, string parameterName)
    {
        if (!transform.IsAffine())
            throw new ArgumentException("Transformation must be a finite affine 4x4 matrix with last row [0, 0, 0, 1].", parameterName);
    }
}
