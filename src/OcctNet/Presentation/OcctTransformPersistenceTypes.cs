using System.Runtime.InteropServices;

namespace OcctNet;

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

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOcctTransformPersistenceState
{
    internal int Mode;
    internal OcctPoint3d Anchor;
    internal int Position;
    internal int OffsetX;
    internal int OffsetY;
}
