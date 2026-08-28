using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctObjectKind { Unknown = 0, Shape = 1, Text = 2, Dimension = 3, Point = 4, Overlay = 5, Manipulator = 6 }
public enum OcctShapeType { Compound = 0, CompSolid = 1, Solid = 2, Shell = 3, Face = 4, Wire = 5, Edge = 6, Vertex = 7, Shape = 8 }
public enum OcctViewOrientation { Isometric = 0, Front = 1, Back = 2, Left = 3, Right = 4, Top = 5, Bottom = 6 }
public enum OcctProjectionType { Orthographic = 0, Perspective = 1 }
public enum OcctDisplayMode { Wireframe = 0, Shaded = 1 }
public enum OcctGradientFillMethod { None = 0, Horizontal = 1, Vertical = 2, Diagonal1 = 3, Diagonal2 = 4, Corner1 = 5, Corner2 = 6, Corner3 = 7, Corner4 = 8, Elliptical = 9 }
public enum OcctSelectionMode { Object = 0, Vertex = 1, Edge = 2, Wire = 3, Face = 4, Shell = 5, Solid = 6 }
public enum OcctRectangleSelectionBehavior { Inclusive = 0, Overlap = 1, Directional = 2 }
public enum OcctBooleanOperation { Fuse = 0, Cut = 1, Common = 2, Section = 3 }
public enum OcctCurveType { Line = 0, Circle = 1, Ellipse = 2, Hyperbola = 3, Parabola = 4, Bezier = 5, BSpline = 6, Offset = 7, Other = 8 }
public enum OcctSurfaceType { Plane = 0, Cylinder = 1, Cone = 2, Sphere = 3, Torus = 4, Bezier = 5, BSpline = 6, Revolution = 7, Extrusion = 8, Offset = 9, Other = 10 }
public enum OcctPointMarker
{
    Point = 0,
    Plus = 1,
    Star = 2,
    X = 3,
    Circle = 4,
    CirclePoint = 5,
    CirclePlus = 6,
    CircleStar = 7,
    CircleX = 8,
    Ring1 = 9,
    Ring2 = 10,
    Ring3 = 11,
    Ball = 12
}
public enum OcctMaterial
{
    Brass = 0, Bronze = 1, Copper = 2, Gold = 3, Pewter = 4, Plastered = 5, Plastified = 6,
    Silver = 7, Steel = 8, Stone = 9, ShinyPlastified = 10, Satin = 11, Metalized = 12,
    Ionized = 13, Chrome = 14, Aluminum = 15, Obsidian = 16, Neon = 17, Jade = 18,
    Charcoal = 19, Water = 20, Glass = 21, Diamond = 22, Transparent = 23, Default = 24
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctPoint3d : IEquatable<OcctPoint3d>
{
    public double X;
    public double Y;
    public double Z;

    public OcctPoint3d(double x, double y, double z) { X = x; Y = y; Z = z; }

    public static OcctPoint3d Origin => new(0, 0, 0);
    public readonly bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);

    public readonly double DistanceTo(OcctPoint3d other) => (this - other).Length;

    public static OcctPoint3d operator +(OcctPoint3d point, OcctVector3d vector) => new(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
    public static OcctPoint3d operator -(OcctPoint3d point, OcctVector3d vector) => new(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);
    public static OcctVector3d operator -(OcctPoint3d left, OcctPoint3d right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    public static bool operator ==(OcctPoint3d left, OcctPoint3d right) => left.Equals(right);
    public static bool operator !=(OcctPoint3d left, OcctPoint3d right) => !left.Equals(right);

    public readonly bool Equals(OcctPoint3d other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    public override readonly bool Equals(object? obj) => obj is OcctPoint3d other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override readonly string ToString() => $"({X:G6}, {Y:G6}, {Z:G6})";
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctVector3d : IEquatable<OcctVector3d>
{
    public double X;
    public double Y;
    public double Z;

    public OcctVector3d(double x, double y, double z) { X = x; Y = y; Z = z; }

    public static OcctVector3d Zero => new(0, 0, 0);
    public static OcctVector3d UnitX => new(1, 0, 0);
    public static OcctVector3d UnitY => new(0, 1, 0);
    public static OcctVector3d UnitZ => new(0, 0, 1);

    public readonly bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);
    public readonly double LengthSquared => X * X + Y * Y + Z * Z;
    public readonly double Length => Math.Sqrt(LengthSquared);

    public readonly OcctVector3d Normalized()
    {
        if (!TryNormalize(out var result)) throw new InvalidOperationException("Vector must be finite and non-zero.");
        return result;
    }

    public readonly bool TryNormalize(out OcctVector3d result)
    {
        var lengthSquared = LengthSquared;
        if (!IsFinite || !double.IsFinite(lengthSquared) || lengthSquared <= 1e-30)
        {
            result = default;
            return false;
        }

        var inverseLength = 1.0 / Math.Sqrt(lengthSquared);
        result = this * inverseLength;
        return true;
    }

    public readonly double Dot(OcctVector3d other) => X * other.X + Y * other.Y + Z * other.Z;
    public readonly OcctVector3d Cross(OcctVector3d other) => new(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);

    public static OcctVector3d operator +(OcctVector3d left, OcctVector3d right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static OcctVector3d operator -(OcctVector3d left, OcctVector3d right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    public static OcctVector3d operator -(OcctVector3d value) => new(-value.X, -value.Y, -value.Z);
    public static OcctVector3d operator *(OcctVector3d value, double factor) => new(value.X * factor, value.Y * factor, value.Z * factor);
    public static OcctVector3d operator *(double factor, OcctVector3d value) => value * factor;
    public static OcctVector3d operator /(OcctVector3d value, double divisor) => new(value.X / divisor, value.Y / divisor, value.Z / divisor);
    public static bool operator ==(OcctVector3d left, OcctVector3d right) => left.Equals(right);
    public static bool operator !=(OcctVector3d left, OcctVector3d right) => !left.Equals(right);

    public readonly bool Equals(OcctVector3d other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    public override readonly bool Equals(object? obj) => obj is OcctVector3d other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override readonly string ToString() => $"<{X:G6}, {Y:G6}, {Z:G6}>";
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctBounds
{
    public double MinX;
    public double MinY;
    public double MinZ;
    public double MaxX;
    public double MaxY;
    public double MaxZ;
    public double SizeX => MaxX - MinX;
    public double SizeY => MaxY - MinY;
    public double SizeZ => MaxZ - MinZ;
    public OcctPoint3d Center => new((MinX + MaxX) / 2, (MinY + MaxY) / 2, (MinZ + MaxZ) / 2);
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctMassProperties
{
    public double Mass;
    public double CenterX;
    public double CenterY;
    public double CenterZ;
    public OcctPoint3d CenterOfMass => new(CenterX, CenterY, CenterZ);
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctDistanceResult
{
    public double Distance;
    public OcctPoint3d PointOnFirst;
    public OcctPoint3d PointOnSecond;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctProjectionRay
{
    public OcctPoint3d Origin;
    public OcctVector3d Direction;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctCameraState
{
    public OcctPoint3d Eye;
    public OcctPoint3d Center;
    public OcctVector3d Up;
    public OcctVector3d Direction;
    public double Scale;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctUvBounds
{
    public double UMin;
    public double UMax;
    public double VMin;
    public double VMax;
}

public readonly record struct OcctFaceEvaluation(OcctPoint3d Point, OcctVector3d Normal);
public readonly record struct OcctEdgeEvaluation(OcctPoint3d Point, OcctVector3d Tangent);

public interface IOcctObject
{
    long Id { get; }
    OcctObjectKind Kind { get; }
    bool IsValid { get; }
}

public readonly record struct OcctShape : IOcctObject
{
    internal OcctShape(long id, long ownerId)
    {
        Id = id;
        OwnerId = ownerId;
    }

    public long Id { get; }
    public OcctObjectKind Kind => OcctObjectKind.Shape;
    public bool IsValid => Id > 0;
    internal long OwnerId { get; }
    public override string ToString() => $"Shape {Id}";
}

public readonly record struct OcctText : IOcctObject
{
    internal OcctText(long id, long ownerId)
    {
        Id = id;
        OwnerId = ownerId;
    }

    public long Id { get; }
    public OcctObjectKind Kind => OcctObjectKind.Text;
    public bool IsValid => Id > 0;
    internal long OwnerId { get; }
}

public readonly record struct OcctDimension : IOcctObject
{
    internal OcctDimension(long id, long ownerId)
    {
        Id = id;
        OwnerId = ownerId;
    }

    public long Id { get; }
    public OcctObjectKind Kind => OcctObjectKind.Dimension;
    public bool IsValid => Id > 0;
    internal long OwnerId { get; }
}

public readonly record struct OcctPoint : IOcctObject
{
    internal OcctPoint(long id, long ownerId)
    {
        Id = id;
        OwnerId = ownerId;
    }

    public long Id { get; }
    public OcctObjectKind Kind => OcctObjectKind.Point;
    public bool IsValid => Id > 0;
    internal long OwnerId { get; }
    public override string ToString() => $"Point {Id}";
}
