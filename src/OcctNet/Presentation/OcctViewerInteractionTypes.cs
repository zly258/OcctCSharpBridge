using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctPixelFormat
{
    Bgra32 = 0,
    Rgba32 = 1
}

public sealed record OcctMarkerPixmap(
    int Width,
    int Height,
    byte[] Pixels,
    OcctPixelFormat PixelFormat = OcctPixelFormat.Bgra32);

public enum OcctZLayer
{
    Bottom = 0,
    Default = 1,
    Top = 2,
    Topmost = 3
}

public enum OcctCornerPosition
{
    LeftLower = 0,
    LeftUpper = 1,
    RightLower = 2,
    RightUpper = 3
}

public sealed record OcctTriedronOptions
{
    public bool Visible { get; init; } = true;
    public OcctCornerPosition Position { get; init; } = OcctCornerPosition.LeftLower;
    public double Scale { get; init; } = 0.08;
    public Color Color { get; init; } = Color.White;
}

public sealed record OcctViewCubeOptions
{
    public bool Visible { get; init; } = true;
    public OcctCornerPosition Position { get; init; } = OcctCornerPosition.RightUpper;
    public int SizePixels { get; init; } = 90;
    public int OffsetX { get; init; } = 10;
    public int OffsetY { get; init; } = 10;
    public double FontHeight { get; init; } = 12.0;
    public string FontName { get; init; } = "Segoe UI";
    public Color TextColor { get; init; } = Color.FromArgb(40, 40, 40);
    public Color BoxColor { get; init; } = Color.FromArgb(230, 230, 230);
    public Color FacetColor { get; init; } = Color.FromArgb(245, 245, 245);
    public double CornerRadius { get; init; } = 0.0;
    public double EdgeWidth { get; init; } = 1.0;
}

public readonly record struct OcctSelectionHitDetail(
    IOcctObject Owner,
    OcctShapeType SubshapeType,
    int SubshapeIndex,
    OcctPoint3d Point,
    double Depth,
    double DistanceToEye)
{
    public bool IsSubshape => SubshapeIndex >= 0;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOcctSelectionHitDetail
{
    internal long OwnerObjectId;
    internal int SubshapeType;
    internal int SubshapeIndex;
    internal OcctPoint3d Point;
    internal double Depth;
    internal double DistanceToEye;
}
