using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctTopologyReferenceStatus
{
    Resolved = 0,
    Ambiguous = 1,
    Removed = 2,
    NotFound = 3,
    InvalidReference = 4
}

/// <summary>
/// Versioned geometric/topological fingerprint for a Vertex, Edge, or Face inside a root shape.
/// RuntimeIndexHint is only a low-weight lookup hint and is never treated as persistent identity.
/// </summary>
public readonly record struct OcctTopologyReference(
    int Version,
    OcctShapeType ShapeType,
    int RuntimeIndexHint,
    OcctCurveType CurveType,
    OcctSurfaceType SurfaceType,
    double Measure,
    OcctPoint3d Center,
    OcctBounds Bounds,
    double Tolerance,
    OcctModelOrientation Orientation,
    int VertexCount,
    int EdgeCount,
    int FaceCount);

/// <summary>
/// Result of resolving a topology reference. Ambiguous results intentionally do not return a shape.
/// </summary>
public readonly record struct OcctTopologyReferenceResult(
    OcctTopologyReferenceStatus Status,
    OcctModelShape? Shape,
    double Score,
    int CandidateCount,
    bool UsedOperationHistory,
    bool RuntimeIndexMatched);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelTopologyReference
{
    internal int Version;
    internal int ShapeType;
    internal int RuntimeIndexHint;
    internal int CurveType;
    internal int SurfaceType;
    internal double Measure;
    internal OcctPoint3d Center;
    internal OcctBounds Bounds;
    internal double Tolerance;
    internal int Orientation;
    internal int VertexCount;
    internal int EdgeCount;
    internal int FaceCount;

    internal readonly OcctTopologyReference ToManaged() => new(
        Version,
        (OcctShapeType)ShapeType,
        RuntimeIndexHint,
        (OcctCurveType)CurveType,
        (OcctSurfaceType)SurfaceType,
        Measure,
        Center,
        Bounds,
        Tolerance,
        (OcctModelOrientation)Orientation,
        VertexCount,
        EdgeCount,
        FaceCount);

    internal static NativeModelTopologyReference FromManaged(OcctTopologyReference value) => new()
    {
        Version = value.Version,
        ShapeType = (int)value.ShapeType,
        RuntimeIndexHint = value.RuntimeIndexHint,
        CurveType = (int)value.CurveType,
        SurfaceType = (int)value.SurfaceType,
        Measure = value.Measure,
        Center = value.Center,
        Bounds = value.Bounds,
        Tolerance = value.Tolerance,
        Orientation = (int)value.Orientation,
        VertexCount = value.VertexCount,
        EdgeCount = value.EdgeCount,
        FaceCount = value.FaceCount
    };
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelTopologyReferenceResult
{
    internal int Status;
    internal long ShapeId;
    internal double Score;
    internal int CandidateCount;
    internal int UsedOperationHistory;
    internal int RuntimeIndexMatched;

    internal readonly OcctTopologyReferenceResult ToManaged(long ownerId) => new(
        (OcctTopologyReferenceStatus)Status,
        ShapeId > 0 ? new OcctModelShape(ShapeId, ownerId) : null,
        Score,
        CandidateCount,
        UsedOperationHistory != 0,
        RuntimeIndexMatched != 0);
}
