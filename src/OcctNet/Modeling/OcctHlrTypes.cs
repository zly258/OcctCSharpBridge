namespace OcctNet;

public readonly record struct OcctHlrProjection(
    OcctVector3d ViewDirection,
    OcctVector3d UpDirection)
{
    public static OcctHlrProjection Front => new(new OcctVector3d(1, 0, 0), new OcctVector3d(0, 0, 1));
    public static OcctHlrProjection Back => new(new OcctVector3d(-1, 0, 0), new OcctVector3d(0, 0, 1));
    public static OcctHlrProjection Left => new(new OcctVector3d(0, 1, 0), new OcctVector3d(0, 0, 1));
    public static OcctHlrProjection Right => new(new OcctVector3d(0, -1, 0), new OcctVector3d(0, 0, 1));
    public static OcctHlrProjection Top => new(new OcctVector3d(0, 0, 1), new OcctVector3d(0, 1, 0));
    public static OcctHlrProjection Bottom => new(new OcctVector3d(0, 0, -1), new OcctVector3d(0, 1, 0));
    public static OcctHlrProjection Isometric => new(new OcctVector3d(1, -1, 1), new OcctVector3d(0, 0, 1));
}

public readonly record struct OcctHlrResult(
    OcctModelShape? VisibleLines,
    OcctModelShape? HiddenLines,
    OcctModelShape? Outlines,
    OcctModelShape? VisibleSharpLines,
    OcctModelShape? HiddenSharpLines);
