using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
public struct OcctParabolaGeometry
{
    public OcctPoint3d Center;
    public OcctVector3d Normal;
    public OcctVector3d XDirection;
    public double FocalLength;
    public double FirstParameter;
    public double LastParameter;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctHyperbolaGeometry
{
    public OcctPoint3d Center;
    public OcctVector3d Normal;
    public OcctVector3d XDirection;
    public double MajorRadius;
    public double MinorRadius;
    public double FirstParameter;
    public double LastParameter;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeBezierCurveInfo
{
    public int Degree;
    public int PoleCount;
    public int Rational;
    public int Closed;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeBezierSurfaceInfo
{
    public int UDegree;
    public int VDegree;
    public int UPoleCount;
    public int VPoleCount;
    public int URational;
    public int VRational;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctExtrusionSurfaceGeometry
{
    public OcctVector3d Direction;
    public OcctCurveType BasisCurveType;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctRevolutionSurfaceGeometry
{
    public OcctPoint3d Origin;
    public OcctVector3d Axis;
    public OcctCurveType BasisCurveType;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctOffsetSurfaceGeometry
{
    public double Offset;
    public OcctSurfaceType BasisSurfaceType;
}

public sealed record OcctBezierCurveData(
    int Degree,
    bool IsRational,
    bool IsClosed,
    IReadOnlyList<OcctPoint3d> Poles,
    IReadOnlyList<double> Weights);

public sealed record OcctBezierSurfaceData(
    int UDegree,
    int VDegree,
    int UPoleCount,
    int VPoleCount,
    bool IsURational,
    bool IsVRational,
    IReadOnlyList<OcctPoint3d> Poles,
    IReadOnlyList<double> Weights)
{
    public int GetIndex(int uIndex, int vIndex)
    {
        if ((uint)uIndex >= (uint)UPoleCount) throw new ArgumentOutOfRangeException(nameof(uIndex));
        if ((uint)vIndex >= (uint)VPoleCount) throw new ArgumentOutOfRangeException(nameof(vIndex));
        return checked(uIndex * VPoleCount + vIndex);
    }

    public OcctPoint3d GetPole(int uIndex, int vIndex) => Poles[GetIndex(uIndex, vIndex)];
    public double GetWeight(int uIndex, int vIndex) => Weights[GetIndex(uIndex, vIndex)];
}
