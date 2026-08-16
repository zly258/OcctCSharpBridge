using System.Drawing;

namespace OcctNet;

/// <summary>
/// View settings applied by a UI adapter before the first OCCT frame of each native-host generation.
/// Runtime changes after the host is ready should use the corresponding <see cref="OcctEngine"/> APIs.
/// </summary>
public sealed record OcctViewportInitializationOptions
{
    public Color BackgroundColor { get; init; } = Color.FromArgb(240, 245, 250);
    public OcctViewOrientation ViewOrientation { get; init; } = OcctViewOrientation.Isometric;
    public OcctProjectionType Projection { get; init; } = OcctProjectionType.Orthographic;
    public bool TriedronVisible { get; init; } = true;
    public bool ViewCubeVisible { get; init; } = true;

    internal void Apply(OcctEngine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);
        if (!Enum.IsDefined(ViewOrientation))
            throw new ArgumentOutOfRangeException(nameof(ViewOrientation));
        if (!Enum.IsDefined(Projection))
            throw new ArgumentOutOfRangeException(nameof(Projection));

        engine.SetBackground(BackgroundColor);
        engine.SetProjection(Projection);
        engine.SetView(ViewOrientation);
        engine.SetTriedronVisible(TriedronVisible);
        engine.SetViewCubeVisible(ViewCubeVisible);
    }
}
