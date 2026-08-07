using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctRenderingMethod
{
    Rasterization = 0,
    RayTracing = 1
}

public enum OcctViewCubeLanguage
{
    English = 0,
    ChineseSimplified = 1
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

[StructLayout(LayoutKind.Sequential)]
public struct OcctViewportState
{
    public int Width;
    public int Height;
    public OcctProjectionType ProjectionType;
    private int _computedMode;
    private int _antialiasingEnabled;
    public int MsaaSamples;
    public OcctRenderingMethod RenderingMethod;
    private int _shadowsEnabled;
    private int _frustumCullingEnabled;
    private int _faceBoundariesVisible;
    public int SelectionTolerance;
    private int _automaticHighlight;
    public double PerspectiveFieldOfView;
    public double RenderResolutionScale;
    public double RenderResolutionDpi;

    public readonly bool ComputedMode => _computedMode != 0;
    public readonly bool AntialiasingEnabled => _antialiasingEnabled != 0;
    public readonly bool ShadowsEnabled => _shadowsEnabled != 0;
    public readonly bool FrustumCullingEnabled => _frustumCullingEnabled != 0;
    public readonly bool FaceBoundariesVisible => _faceBoundariesVisible != 0;
    public readonly bool AutomaticHighlight => _automaticHighlight != 0;
}
