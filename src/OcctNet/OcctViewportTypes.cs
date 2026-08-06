using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctRenderingMethod
{
    Rasterization = 0,
    RayTracing = 1
}

public enum OcctZUpViewOrientation
{
    Front = 0,
    Back = 1,
    Left = 2,
    Right = 3,
    Top = 4,
    Bottom = 5,
    IsometricXNegativeYNegative = 6,
    IsometricXPositiveYNegative = 7,
    IsometricXNegativeYPositive = 8,
    IsometricXPositiveYPositive = 9
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctProjectionRay
{
    public OcctPoint3d Origin;
    public OcctVector3d Direction;

    public OcctProjectionRay(OcctPoint3d origin, OcctVector3d direction)
    {
        Origin = origin;
        Direction = direction;
    }
}
