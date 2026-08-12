namespace OcctNet;

/// <summary>
/// Pure managed geometry helpers for common CAD calculations that do not require a native OCCT call.
/// </summary>
public static partial class OcctGeometryExtensions
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
}
