using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
public struct OcctEdgeProjectionResult
{
    public OcctPoint3d Point;
    public double NormalizedParameter;
    public double Distance;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctFaceProjectionResult
{
    public OcctPoint3d Point;
    public double U;
    public double V;
    public double Distance;
}
