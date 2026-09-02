using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
public struct OcctEdgeProjectionResult
{
    public OcctPoint3d Point;
    public OcctVector3d Tangent;
    public double NormalizedParameter;
    public double Distance;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctFaceProjectionResult
{
    public OcctPoint3d Point;
    public OcctVector3d Normal;
    public double U;
    public double V;
    public double Distance;
}

public readonly record struct OcctEdgeTangentPoint(
    OcctPoint3d Point,
    double NormalizedParameter);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeEdgeTangentPoint
{
    internal OcctPoint3d Point;
    internal double NormalizedParameter;

    internal readonly OcctEdgeTangentPoint ToManaged() =>
        new(Point, NormalizedParameter);
}
