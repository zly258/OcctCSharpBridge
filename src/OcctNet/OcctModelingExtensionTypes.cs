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
