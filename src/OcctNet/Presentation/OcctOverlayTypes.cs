using System.Drawing;

namespace OcctNet;

public enum OcctOverlayPrimitiveType
{
    Line = 0,
    Polyline = 1,
    Marker = 2,
    Text = 3
}

public enum OcctOverlayLinePattern
{
    Solid = 0,
    Dashed = 1,
    Dotted = 2,
    DashDot = 3
}

public readonly record struct OcctOverlay : IOcctObject
{
    internal OcctOverlay(long id, long ownerId, OcctOverlayPrimitiveType primitiveType)
    {
        Id = id;
        OwnerId = ownerId;
        PrimitiveType = primitiveType;
    }

    public long Id { get; }
    public OcctObjectKind Kind => OcctObjectKind.Overlay;
    public bool IsValid => Id > 0;
    public OcctOverlayPrimitiveType PrimitiveType { get; }
    internal long OwnerId { get; }
    public override string ToString() => $"Overlay {Id} ({PrimitiveType})";
}

public sealed record OcctOverlayLineStyle
{
    public Color Color { get; init; } = Color.Gold;
    public double Width { get; init; } = 1.0;
    public OcctOverlayLinePattern Pattern { get; init; } = OcctOverlayLinePattern.Solid;
}

public sealed record OcctOverlayMarkerStyle
{
    public Color Color { get; init; } = Color.Gold;
    public OcctPointMarker Marker { get; init; } = OcctPointMarker.Plus;
    public double Scale { get; init; } = 8.0;
}

public sealed record OcctOverlayTextStyle
{
    public Color Color { get; init; } = Color.White;
    public double Height { get; init; } = 16.0;
    public bool Zoomable { get; init; }
    public string FontName { get; init; } = "Arial";
}
