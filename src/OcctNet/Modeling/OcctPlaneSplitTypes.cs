namespace OcctNet;

public readonly record struct OcctPlane3d(OcctPoint3d Origin, OcctVector3d Normal);

public readonly record struct OcctPlaneSplitResult(
    OcctModelShape? Positive,
    OcctModelShape? Negative,
    OcctModelShape? Section);
