using OcctNet;

namespace OcctNet.DesktopConsumerMatrix;

internal static class CompileBoundary
{
    internal static Type[] PublicTypes =>
    [
        typeof(OcctViewportControl),
        typeof(OcctWpfViewport)
    ];
}
