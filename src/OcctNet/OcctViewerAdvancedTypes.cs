using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctHighlightStyleKind
{
    Dynamic = 0,
    Selected = 1,
    LocalDynamic = 2,
    LocalSelected = 3
}

public sealed record OcctHighlightStyle
{
    public Color Color { get; init; } = Color.Cyan;
    public double Transparency { get; init; }
    public double LineWidth { get; init; } = 2.0;
    public OcctDisplayMode? DisplayMode { get; init; }
    public OcctZLayer? ZLayer { get; init; }
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOcctHighlightStyleSettings
{
    internal double R;
    internal double G;
    internal double B;
    internal double Transparency;
    internal double LineWidth;
    internal int DisplayMode;
    internal int ZLayer;
}
