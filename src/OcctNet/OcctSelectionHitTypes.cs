using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOcctSelectionHit
{
    public long OwnerObjectId;
    public int SubshapeType;
    public int SubshapeIndex;
}

/// <summary>
/// Structured identity of a selected or detected AIS entity.
/// Subshape indices are runtime topology indices and are not persistent naming.
/// </summary>
public readonly record struct OcctSelectionHit(
    IOcctObject Owner,
    OcctShapeType SubshapeType,
    int SubshapeIndex)
{
    public bool IsSubshape => SubshapeIndex >= 0;
}
