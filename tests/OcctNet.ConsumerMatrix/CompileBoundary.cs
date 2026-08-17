using OcctNet;

namespace OcctNet.ConsumerMatrix;

internal static class CompileBoundary
{
    internal static Type[] PublicTypes =>
    [
        typeof(OcctEngine),
        typeof(OcctModelingSession),
        typeof(OcctPoint3d),
        typeof(OcctAvaloniaViewport)
    ];
}
