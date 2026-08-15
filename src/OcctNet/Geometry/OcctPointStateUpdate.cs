using System.Runtime.InteropServices;

namespace OcctNet;

public readonly record struct OcctPointStateUpdate(
    OcctPoint Point,
    OcctPoint3d Position,
    bool Visible);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOcctPointStateUpdate
{
    internal long PointId;
    internal OcctPoint3d Position;
    internal int Visible;
}
