namespace OcctNet;

[Flags]
public enum OcctPolygonOffsetMode
{
    Off = 0,
    Fill = 1,
    Line = 2,
    Point = 4,
    All = Fill | Line | Point
}

public enum OcctDepthBiasPreset
{
    None = 0,
    Default = 1,
    CoincidentFaces = 2,
    Aggressive = 3
}

public readonly record struct OcctPolygonOffsetSettings(
    OcctPolygonOffsetMode Mode,
    double Factor,
    double Units);

public readonly record struct OcctAutoZFitSettings(
    bool Enabled,
    double ScaleFactor);
