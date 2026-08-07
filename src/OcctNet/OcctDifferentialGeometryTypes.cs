using System.Runtime.InteropServices;

namespace OcctNet;

public readonly record struct OcctModelParameterRange(
    double FirstParameter,
    double LastParameter,
    bool IsClosed,
    bool IsPeriodic,
    double Period)
{
    public double Length => LastParameter - FirstParameter;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelParameterRange
{
    public double FirstParameter;
    public double LastParameter;
    public int IsClosed;
    public int IsPeriodic;
    public double Period;

    public readonly OcctModelParameterRange ToManaged() => new(
        FirstParameter,
        LastParameter,
        IsClosed != 0,
        IsPeriodic != 0,
        Period);
}

public readonly record struct OcctModelCurveDifferential(
    double Parameter,
    OcctPoint3d Point,
    OcctVector3d FirstDerivative,
    OcctVector3d SecondDerivative);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelCurveDifferential
{
    public double Parameter;
    public OcctPoint3d Point;
    public OcctVector3d FirstDerivative;
    public OcctVector3d SecondDerivative;

    public readonly OcctModelCurveDifferential ToManaged() => new(
        Parameter,
        Point,
        FirstDerivative,
        SecondDerivative);
}

public readonly record struct OcctModelCurveCurvature(
    double Parameter,
    OcctPoint3d Point,
    OcctVector3d Tangent,
    OcctVector3d Normal,
    OcctPoint3d CenterOfCurvature,
    double Curvature,
    bool HasTangent,
    bool HasNormal,
    bool HasCenterOfCurvature)
{
    public double RadiusOfCurvature => Math.Abs(Curvature) > double.Epsilon
        ? 1.0 / Math.Abs(Curvature)
        : double.PositiveInfinity;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelCurveCurvature
{
    public double Parameter;
    public OcctPoint3d Point;
    public OcctVector3d Tangent;
    public OcctVector3d Normal;
    public OcctPoint3d CenterOfCurvature;
    public double Curvature;
    public int HasTangent;
    public int HasNormal;
    public int HasCenterOfCurvature;

    public readonly OcctModelCurveCurvature ToManaged() => new(
        Parameter,
        Point,
        Tangent,
        Normal,
        CenterOfCurvature,
        Curvature,
        HasTangent != 0,
        HasNormal != 0,
        HasCenterOfCurvature != 0);
}

public readonly record struct OcctModelSurfacePeriodicity(
    bool IsUClosed,
    bool IsVClosed,
    bool IsUPeriodic,
    bool IsVPeriodic,
    double UPeriod,
    double VPeriod);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelSurfacePeriodicity
{
    public int IsUClosed;
    public int IsVClosed;
    public int IsUPeriodic;
    public int IsVPeriodic;
    public double UPeriod;
    public double VPeriod;

    public readonly OcctModelSurfacePeriodicity ToManaged() => new(
        IsUClosed != 0,
        IsVClosed != 0,
        IsUPeriodic != 0,
        IsVPeriodic != 0,
        UPeriod,
        VPeriod);
}

public readonly record struct OcctModelSurfaceDifferential(
    double U,
    double V,
    OcctPoint3d Point,
    OcctVector3d Normal,
    OcctVector3d UDerivative,
    OcctVector3d VDerivative,
    OcctVector3d USecondDerivative,
    OcctVector3d VSecondDerivative,
    OcctVector3d UvDerivative,
    bool HasNormal);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelSurfaceDifferential
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
    public int HasNormal;

    public readonly OcctModelSurfaceDifferential ToManaged() => new(
        U,
        V,
        Point,
        Normal,
        UDerivative,
        VDerivative,
        USecondDerivative,
        VSecondDerivative,
        UvDerivative,
        HasNormal != 0);
}

public readonly record struct OcctModelSurfaceCurvature(
    double U,
    double V,
    OcctPoint3d Point,
    OcctVector3d Normal,
    OcctVector3d MaximumDirection,
    OcctVector3d MinimumDirection,
    double MaximumCurvature,
    double MinimumCurvature,
    double MeanCurvature,
    double GaussianCurvature,
    bool IsUmbilic,
    bool HasNormal,
    bool HasCurvature);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelSurfaceCurvature
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
    public int IsUmbilic;
    public int HasNormal;
    public int HasCurvature;

    public readonly OcctModelSurfaceCurvature ToManaged() => new(
        U,
        V,
        Point,
        Normal,
        MaximumDirection,
        MinimumDirection,
        MaximumCurvature,
        MinimumCurvature,
        MeanCurvature,
        GaussianCurvature,
        IsUmbilic != 0,
        HasNormal != 0,
        HasCurvature != 0);
}
