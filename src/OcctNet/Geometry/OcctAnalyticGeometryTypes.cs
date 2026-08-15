using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
public struct OcctLineGeometry
{
    public OcctPoint3d Origin;
    public OcctVector3d Direction;
    public double FirstParameter;
    public double LastParameter;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctCircleGeometry
{
    public OcctPoint3d Center;
    public OcctVector3d Normal;
    public OcctVector3d XDirection;
    public double Radius;
    public double FirstParameter;
    public double LastParameter;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctEllipseGeometry
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
public struct OcctPlaneGeometry
{
    public OcctPoint3d Origin;
    public OcctVector3d Normal;
    public OcctVector3d XDirection;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctCylinderGeometry
{
    public OcctPoint3d Origin;
    public OcctVector3d Axis;
    public OcctVector3d XDirection;
    public double Radius;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctConeGeometry
{
    public OcctPoint3d Apex;
    public OcctVector3d Axis;
    public OcctVector3d XDirection;
    public double ReferenceRadius;
    public double SemiAngleRadians;

    public readonly double SemiAngleDegrees => SemiAngleRadians * 180.0 / Math.PI;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctSphereGeometry
{
    public OcctPoint3d Center;
    public OcctVector3d Axis;
    public OcctVector3d XDirection;
    public double Radius;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctTorusGeometry
{
    public OcctPoint3d Center;
    public OcctVector3d Axis;
    public OcctVector3d XDirection;
    public double MajorRadius;
    public double MinorRadius;
}
