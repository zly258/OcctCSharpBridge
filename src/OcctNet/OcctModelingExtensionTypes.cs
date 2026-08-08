using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctJoinType
{
    Arc = 0,
    Tangent = 1,
    Intersection = 2
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctOrientedBounds
{
    public OcctPoint3d Center;
    public OcctVector3d XDirection;
    public OcctVector3d YDirection;
    public OcctVector3d ZDirection;
    public double HalfSizeX;
    public double HalfSizeY;
    public double HalfSizeZ;

    public readonly double SizeX => HalfSizeX * 2.0;
    public readonly double SizeY => HalfSizeY * 2.0;
    public readonly double SizeZ => HalfSizeZ * 2.0;
    public readonly double Volume => SizeX * SizeY * SizeZ;

    public readonly bool IsFinite =>
        Center.IsFinite &&
        XDirection.IsFinite &&
        YDirection.IsFinite &&
        ZDirection.IsFinite &&
        double.IsFinite(HalfSizeX) &&
        double.IsFinite(HalfSizeY) &&
        double.IsFinite(HalfSizeZ);
}

/// <summary>
/// Immutable snapshot of the OCCT B-Spline data carried by an edge curve.
/// Pole and knot lists use zero-based managed indexing.
/// </summary>
public sealed class OcctBSplineCurveData
{
    internal OcctBSplineCurveData(
        int degree,
        bool isRational,
        bool isPeriodic,
        OcctPoint3d[] poles,
        double[] weights,
        double[] knots,
        int[] multiplicities)
    {
        Degree = degree;
        IsRational = isRational;
        IsPeriodic = isPeriodic;
        Poles = Array.AsReadOnly((OcctPoint3d[])poles.Clone());
        Weights = Array.AsReadOnly((double[])weights.Clone());
        Knots = Array.AsReadOnly((double[])knots.Clone());
        Multiplicities = Array.AsReadOnly((int[])multiplicities.Clone());
    }

    public int Degree { get; }
    public bool IsRational { get; }
    public bool IsPeriodic { get; }
    public IReadOnlyList<OcctPoint3d> Poles { get; }
    public IReadOnlyList<double> Weights { get; }
    public IReadOnlyList<double> Knots { get; }
    public IReadOnlyList<int> Multiplicities { get; }
    public int PoleCount => Poles.Count;
    public int KnotCount => Knots.Count;
}

[StructLayout(LayoutKind.Sequential)]
internal struct OcctModelBSplineCurveInfoNative
{
    internal int Degree;
    internal int PoleCount;
    internal int KnotCount;
    internal int Rational;
    internal int Periodic;
}
