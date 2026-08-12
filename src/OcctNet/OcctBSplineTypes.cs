using System.Runtime.InteropServices;

namespace OcctNet;

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

/// <summary>
/// Immutable snapshot of the OCCT B-Spline data carried by a face surface.
/// Pole and weight grids use zero-based managed indexing. Flat pole storage is U-major
/// with V varying fastest: flatIndex = uIndex * VPoleCount + vIndex.
/// </summary>
public sealed class OcctBSplineSurfaceData
{
    internal OcctBSplineSurfaceData(
        int uDegree,
        int vDegree,
        bool isURational,
        bool isVRational,
        bool isUPeriodic,
        bool isVPeriodic,
        int uPoleCount,
        int vPoleCount,
        OcctPoint3d[] poles,
        double[] weights,
        double[] uKnots,
        int[] uMultiplicities,
        double[] vKnots,
        int[] vMultiplicities)
    {
        UDegree = uDegree;
        VDegree = vDegree;
        IsURational = isURational;
        IsVRational = isVRational;
        IsUPeriodic = isUPeriodic;
        IsVPeriodic = isVPeriodic;
        UPoleCount = uPoleCount;
        VPoleCount = vPoleCount;
        Poles = Array.AsReadOnly((OcctPoint3d[])poles.Clone());
        Weights = Array.AsReadOnly((double[])weights.Clone());
        UKnots = Array.AsReadOnly((double[])uKnots.Clone());
        UMultiplicities = Array.AsReadOnly((int[])uMultiplicities.Clone());
        VKnots = Array.AsReadOnly((double[])vKnots.Clone());
        VMultiplicities = Array.AsReadOnly((int[])vMultiplicities.Clone());
    }

    public int UDegree { get; }
    public int VDegree { get; }
    public bool IsURational { get; }
    public bool IsVRational { get; }
    public bool IsUPeriodic { get; }
    public bool IsVPeriodic { get; }
    public int UPoleCount { get; }
    public int VPoleCount { get; }
    public int PoleCount => checked(UPoleCount * VPoleCount);
    public int UKnotCount => UKnots.Count;
    public int VKnotCount => VKnots.Count;
    public IReadOnlyList<OcctPoint3d> Poles { get; }
    public IReadOnlyList<double> Weights { get; }
    public IReadOnlyList<double> UKnots { get; }
    public IReadOnlyList<int> UMultiplicities { get; }
    public IReadOnlyList<double> VKnots { get; }
    public IReadOnlyList<int> VMultiplicities { get; }

    public OcctPoint3d GetPole(int uIndex, int vIndex) => Poles[GetPoleIndex(uIndex, vIndex)];

    public double GetWeight(int uIndex, int vIndex) => Weights[GetPoleIndex(uIndex, vIndex)];

    private int GetPoleIndex(int uIndex, int vIndex)
    {
        if ((uint)uIndex >= (uint)UPoleCount)
            throw new ArgumentOutOfRangeException(nameof(uIndex));
        if ((uint)vIndex >= (uint)VPoleCount)
            throw new ArgumentOutOfRangeException(nameof(vIndex));
        return checked(uIndex * VPoleCount + vIndex);
    }
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

[StructLayout(LayoutKind.Sequential)]
internal struct OcctModelBSplineSurfaceInfoNative
{
    internal int UDegree;
    internal int VDegree;
    internal int UPoleCount;
    internal int VPoleCount;
    internal int UKnotCount;
    internal int VKnotCount;
    internal int URational;
    internal int VRational;
    internal int UPeriodic;
    internal int VPeriodic;
}
