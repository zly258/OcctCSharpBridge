using System.Drawing;

namespace OcctNet;

public readonly record struct OcctObjectDescriptor(long Id, OcctObjectKind Kind);

public sealed record OcctObjectAppearance
{
    public Color Color { get; init; } = Color.White;
    public double Transparency { get; init; }
    public bool Visible { get; init; } = true;
    public OcctDisplayMode DisplayMode { get; init; } = OcctDisplayMode.Shaded;
    public double LineWidth { get; init; } = 1.0;
    public OcctMaterial Material { get; init; } = OcctMaterial.Default;
}
