namespace OcctNet;

public sealed class OcctDraftOptions
{
    public static OcctDraftOptions Default => new();
    public double AngleDegrees { get; set; } = 5.0;
    public OcctVector3d PullDirection { get; set; } = new(0, 0, 1);
    public OcctPoint3d NeutralPlanePoint { get; set; } = new(0, 0, 0);
    public OcctVector3d NeutralPlaneNormal { get; set; } = new(0, 0, 1);
}
