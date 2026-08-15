using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctIntersectionKind
{
    Point = 0,
    Overlap = 1
}

/// <summary>
/// A bounded Edge/Edge common part. Parameters are native curve parameters, not normalized values.
/// For a point intersection, start and end points/parameters are equal.
/// </summary>
public readonly record struct OcctEdgeIntersection(
    OcctIntersectionKind Kind,
    OcctPoint3d StartPoint,
    OcctPoint3d EndPoint,
    double FirstParameterStart,
    double FirstParameterEnd,
    double SecondParameterStart,
    double SecondParameterEnd);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelEdgeIntersection
{
    internal int Kind;
    internal OcctPoint3d StartPoint;
    internal OcctPoint3d EndPoint;
    internal double FirstParameterStart;
    internal double FirstParameterEnd;
    internal double SecondParameterStart;
    internal double SecondParameterEnd;

    internal readonly OcctEdgeIntersection ToManaged() => new(
        (OcctIntersectionKind)Kind,
        StartPoint,
        EndPoint,
        FirstParameterStart,
        FirstParameterEnd,
        SecondParameterStart,
        SecondParameterEnd);
}
