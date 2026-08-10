using System.Runtime.InteropServices;

namespace OcctNet;

public readonly record struct OcctInertiaProperties(
    double Mass,
    OcctPoint3d CenterOfMass,
    double Ixx,
    double Iyy,
    double Izz,
    double Ixy,
    double Ixz,
    double Iyz,
    double PrincipalMoment1,
    double PrincipalMoment2,
    double PrincipalMoment3,
    OcctVector3d PrincipalAxis1,
    OcctVector3d PrincipalAxis2,
    OcctVector3d PrincipalAxis3,
    double RadiusOfGyration1,
    double RadiusOfGyration2,
    double RadiusOfGyration3,
    bool HasSymmetryAxis,
    bool HasSymmetryPoint);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelInertiaProperties
{
    internal double Mass;
    internal OcctPoint3d CenterOfMass;
    internal double Ixx;
    internal double Iyy;
    internal double Izz;
    internal double Ixy;
    internal double Ixz;
    internal double Iyz;
    internal double PrincipalMoment1;
    internal double PrincipalMoment2;
    internal double PrincipalMoment3;
    internal OcctVector3d PrincipalAxis1;
    internal OcctVector3d PrincipalAxis2;
    internal OcctVector3d PrincipalAxis3;
    internal double RadiusOfGyration1;
    internal double RadiusOfGyration2;
    internal double RadiusOfGyration3;
    internal int HasSymmetryAxis;
    internal int HasSymmetryPoint;

    internal readonly OcctInertiaProperties ToManaged() => new(
        Mass,
        CenterOfMass,
        Ixx,
        Iyy,
        Izz,
        Ixy,
        Ixz,
        Iyz,
        PrincipalMoment1,
        PrincipalMoment2,
        PrincipalMoment3,
        PrincipalAxis1,
        PrincipalAxis2,
        PrincipalAxis3,
        RadiusOfGyration1,
        RadiusOfGyration2,
        RadiusOfGyration3,
        HasSymmetryAxis != 0,
        HasSymmetryPoint != 0);
}
