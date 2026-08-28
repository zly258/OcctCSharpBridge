using System.Runtime.InteropServices;

namespace OcctNet;

public readonly record struct OcctCurveCurveExtremum(
    OcctPoint3d PointOnFirst,
    OcctPoint3d PointOnSecond,
    double Distance,
    double FirstParameter,
    double SecondParameter);

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
