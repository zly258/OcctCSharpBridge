using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public sealed record OcctViewClipPlane
{
    public OcctPoint3d Point { get; init; } = OcctPoint3d.Origin;
    public OcctVector3d Normal { get; init; } = OcctVector3d.UnitZ;
    public bool Enabled { get; init; } = true;
    public bool Capping { get; init; }
    public Color CappingColor { get; init; } = Color.Gray;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOcctViewClipPlane
{
    internal OcctPoint3d Point;
    internal OcctVector3d Normal;
    internal int Enabled;
    internal int Capping;
    internal double CappingR;
    internal double CappingG;
    internal double CappingB;
}
