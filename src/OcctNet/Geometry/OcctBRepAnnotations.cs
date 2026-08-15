using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctTextHorizontalAlignment
{
    Left = 0,
    Center = 1,
    Right = 2
}

public enum OcctTextVerticalAlignment
{
    Bottom = 0,
    Center = 1,
    Top = 2
}

public readonly record struct OcctBRepTextOptions(
    OcctPoint3d Position,
    OcctVector3d Normal,
    OcctVector3d XDirection,
    double Height,
    double ExtrusionDepth,
    string FontName,
    bool Bold,
    bool Italic,
    OcctTextHorizontalAlignment HorizontalAlignment,
    OcctTextVerticalAlignment VerticalAlignment)
{
    public static OcctBRepTextOptions Default => new(
        OcctPoint3d.Origin,
        OcctVector3d.UnitZ,
        OcctVector3d.UnitX,
        10,
        0,
        string.Empty,
        false,
        false,
        OcctTextHorizontalAlignment.Left,
        OcctTextVerticalAlignment.Bottom);
}

public readonly record struct OcctBRepAnnotationOptions(
    double Offset,
    double TextHeight,
    double ArrowSize,
    string FontName)
{
    public static OcctBRepAnnotationOptions Default => new(20, 5, 3, string.Empty);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeBRepTextOptions
{
    public uint StructSize;
    public uint ApiVersion;
    public OcctPoint3d Position;
    public OcctVector3d Normal;
    public OcctVector3d XDirection;
    public double Height;
    public double ExtrusionDepth;
    public int Bold;
    public int Italic;
    public int HorizontalAlignment;
    public int VerticalAlignment;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeBRepAnnotationOptions
{
    public uint StructSize;
    public uint ApiVersion;
    public double Offset;
    public double TextHeight;
    public double ArrowSize;
}
