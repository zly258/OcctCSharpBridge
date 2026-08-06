using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelParameterRange
{
    public double FirstParameter;
    public double LastParameter;
    public int NativeIsClosed;
    public int NativeIsPeriodic;
    public double Period;

    public readonly bool IsClosed => NativeIsClosed != 0;
    public readonly bool IsPeriodic => NativeIsPeriodic != 0;
    public readonly double Length => LastParameter - FirstParameter;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelCurveDifferential
{
    public double Parameter;
    public OcctPoint3d Point;
    public OcctVector3d FirstDerivative;
    public OcctVector3d SecondDerivative;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelCurveCurvature
{
    public double Parameter;
    public OcctPoint3d Point;
    public OcctVector3d Tangent;
    public OcctVector3d Normal;
    public OcctPoint3d CenterOfCurvature;
    public double Curvature;
    public int NativeHasTangent;
    public int NativeHasNormal;
    public int NativeHasCenterOfCurvature;

    public readonly bool HasTangent => NativeHasTangent != 0;
    public readonly bool HasNormal => NativeHasNormal != 0;
    public readonly bool HasCenterOfCurvature => NativeHasCenterOfCurvature != 0;
    public readonly double RadiusOfCurvature => Math.Abs(Curvature) > double.Epsilon
        ? 1.0 / Math.Abs(Curvature)
        : double.PositiveInfinity;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelSurfacePeriodicity
{
    public int NativeIsUClosed;
    public int NativeIsVClosed;
    public int NativeIsUPeriodic;
    public int NativeIsVPeriodic;
    public double UPeriod;
    public double VPeriod;

    public readonly bool IsUClosed => NativeIsUClosed != 0;
    public readonly bool IsVClosed => NativeIsVClosed != 0;
    public readonly bool IsUPeriodic => NativeIsUPeriodic != 0;
    public readonly bool IsVPeriodic => NativeIsVPeriodic != 0;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelSurfaceDifferential
{
    public double U;
    public double V;
    public OcctPoint3d Point;
    public OcctVector3d Normal;
    public OcctVector3d UDerivative;
    public OcctVector3d VDerivative;
    public OcctVector3d USecondDerivative;
    public OcctVector3d VSecondDerivative;
    public OcctVector3d UvDerivative;
    public int NativeHasNormal;

    public readonly bool HasNormal => NativeHasNormal != 0;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelSurfaceCurvature
{
    public double U;
    public double V;
    public OcctPoint3d Point;
    public OcctVector3d Normal;
    public OcctVector3d MaximumDirection;
    public OcctVector3d MinimumDirection;
    public double MaximumCurvature;
    public double MinimumCurvature;
    public double MeanCurvature;
    public double GaussianCurvature;
    public int NativeIsUmbilic;
    public int NativeHasNormal;
    public int NativeHasCurvature;

    public readonly bool IsUmbilic => NativeIsUmbilic != 0;
    public readonly bool HasNormal => NativeHasNormal != 0;
    public readonly bool HasCurvature => NativeHasCurvature != 0;
}
