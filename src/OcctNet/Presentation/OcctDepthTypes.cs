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

public readonly record struct OcctPolygonOffsetSettings(
    OcctPolygonOffsetMode Mode,
    double Factor,
    double Units);

public readonly record struct OcctAutoZFitSettings(
    bool Enabled,
    double ScaleFactor);
