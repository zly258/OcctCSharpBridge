using System.Runtime.InteropServices;

namespace OcctNet;

public readonly record struct OcctCurveCurveExtremum(
    OcctPoint3d PointOnFirst,
    OcctPoint3d PointOnSecond,
    double Distance,
    double FirstParameter,
    double SecondParameter);

public readonly record struct OcctCurveSurfaceExtremum(
    OcctPoint3d PointOnCurve,
    OcctPoint3d PointOnSurface,
    double Distance,
    double CurveParameter,
    double U,
    double V);

public readonly record struct OcctSurfaceSurfaceExtremum(
    OcctPoint3d PointOnFirst,
    OcctPoint3d PointOnSecond,
    double Distance,
    double FirstU,
    double FirstV,
    double SecondU,
    double SecondV);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelCurveCurveExtremum
{
    internal OcctPoint3d PointOnFirst;
    internal OcctPoint3d PointOnSecond;
    internal double Distance;
    internal double FirstParameter;
    internal double SecondParameter;

    internal readonly OcctCurveCurveExtremum ToManaged() =>
        new(PointOnFirst, PointOnSecond, Distance, FirstParameter, SecondParameter);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelCurveSurfaceExtremum
{
    internal OcctPoint3d PointOnCurve;
    internal OcctPoint3d PointOnSurface;
    internal double Distance;
    internal double CurveParameter;
    internal double U;
    internal double V;

    internal readonly OcctCurveSurfaceExtremum ToManaged() =>
        new(PointOnCurve, PointOnSurface, Distance, CurveParameter, U, V);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelSurfaceSurfaceExtremum
{
    internal OcctPoint3d PointOnFirst;
    internal OcctPoint3d PointOnSecond;
    internal double Distance;
    internal double FirstU;
    internal double FirstV;
    internal double SecondU;
    internal double SecondV;

    internal readonly OcctSurfaceSurfaceExtremum ToManaged() =>
        new(PointOnFirst, PointOnSecond, Distance, FirstU, FirstV, SecondU, SecondV);
}
