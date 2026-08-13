namespace OcctNet;

public sealed record OcctDetectionFilter
{
    public IReadOnlyCollection<IOcctObject>? Owners { get; init; }
    public IReadOnlyCollection<OcctObjectKind>? ObjectKinds { get; init; }
    public IReadOnlyCollection<OcctShapeType>? ShapeTypes { get; init; }
    public bool IncludeWholeObjects { get; init; } = true;
}
