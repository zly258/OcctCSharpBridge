using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctSelectionModeConcurrency
{
    Single = 0,
    GlobalOrLocal = 1,
    Multiple = 2
}

public enum OcctTransformPersistenceMode
{
    None = 0,
    Zoom = 1,
    Rotate = 2,
    ZoomRotate = 3,
    Screen2d = 4,
    Triedron = 5
}

public readonly record struct OcctTransformPersistenceState(
    OcctTransformPersistenceMode Mode,
    OcctPoint3d Anchor,
    OcctCornerPosition Position,
    int OffsetX,
    int OffsetY)
{
    public bool Enabled => Mode != OcctTransformPersistenceMode.None;
    public bool IsScreenAnchored =>
        Mode is OcctTransformPersistenceMode.Screen2d or OcctTransformPersistenceMode.Triedron;
}

public sealed record OcctViewClipPlane
{
    public OcctPoint3d Point { get; init; } = OcctPoint3d.Origin;
    public OcctVector3d Normal { get; init; } = OcctVector3d.UnitZ;
    public bool Enabled { get; init; } = true;
    public bool Capping { get; init; }
    public Color CappingColor { get; init; } = Color.Gray;
}

public readonly record struct OcctPointStateUpdate(
    OcctPoint Point,
    OcctPoint3d Position,
    bool Visible);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOcctTransformPersistenceState
{
    internal int Mode;
    internal OcctPoint3d Anchor;
    internal int Position;
    internal int OffsetX;
    internal int OffsetY;
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

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOcctPointStateUpdate
{
    internal long PointId;
    internal OcctPoint3d Position;
    internal int Visible;
}
