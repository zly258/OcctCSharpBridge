namespace OcctNet;

public enum OcctManipulatorMode
{
    None = 0,
    Translation = 1,
    Rotation = 2,
    Scaling = 3,
    TranslationPlane = 4
}

public enum OcctManipulatorSkin
{
    Shaded = 0,
    Flat = 1
}

public sealed record OcctManipulatorAttachOptions
{
    public bool AdjustPosition { get; init; } = true;
    public bool AdjustSize { get; init; }
    public bool EnableModes { get; init; } = true;
}

public readonly record struct OcctManipulator : IOcctObject
{
    internal OcctManipulator(long id, long ownerId)
    {
        Id = id;
        OwnerId = ownerId;
    }

    public long Id { get; }
    public OcctObjectKind Kind => OcctObjectKind.Manipulator;
    public bool IsValid => Id > 0;
    internal long OwnerId { get; }
    public override string ToString() => $"Manipulator {Id}";
}

public readonly record struct OcctManipulatorState(
    bool IsAttached,
    OcctManipulatorMode ActiveMode,
    int ActiveAxisIndex,
    bool HasActiveTransformation,
    bool ModeActivationOnDetection,
    bool ZoomPersistence,
    OcctManipulatorSkin Skin,
    OcctPoint3d Origin,
    OcctVector3d Normal,
    OcctVector3d XDirection,
    double Size);
