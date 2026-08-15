using System.Runtime.InteropServices;

namespace OcctNet;

public readonly record struct OcctFaceAnalysisInfo(
    OcctModelShape Face,
    OcctSurfaceType SurfaceType,
    OcctModelOrientation Orientation,
    double Area,
    double Tolerance,
    OcctUvBounds UvBounds,
    OcctBounds Bounds,
    int EdgeCount,
    int WireCount)
{
    public bool IsAnalytic => SurfaceType is
        OcctSurfaceType.Plane or
        OcctSurfaceType.Cylinder or
        OcctSurfaceType.Cone or
        OcctSurfaceType.Sphere or
        OcctSurfaceType.Torus;

    public bool IsFreeform => SurfaceType is OcctSurfaceType.Bezier or OcctSurfaceType.BSpline;
}

public sealed class OcctFaceAnalysisResult
{
    private readonly IReadOnlyDictionary<OcctSurfaceType, int> _surfaceTypeCounts;

    internal OcctFaceAnalysisResult(OcctModelShape root, OcctFaceAnalysisInfo[] faces)
    {
        Root = root;
        Faces = Array.AsReadOnly((OcctFaceAnalysisInfo[])faces.Clone());
        TotalArea = faces.Sum(static face => face.Area);
        MaximumTolerance = faces.Length == 0 ? 0.0 : faces.Max(static face => face.Tolerance);
        _surfaceTypeCounts = faces
            .GroupBy(static face => face.SurfaceType)
            .ToDictionary(static group => group.Key, static group => group.Count());
    }

    public OcctModelShape Root { get; }
    public IReadOnlyList<OcctFaceAnalysisInfo> Faces { get; }
    public int FaceCount => Faces.Count;
    public double TotalArea { get; }
    public double MaximumTolerance { get; }
    public IReadOnlyDictionary<OcctSurfaceType, int> SurfaceTypeCounts => _surfaceTypeCounts;

    public IReadOnlyList<OcctFaceAnalysisInfo> GetFacesBySurfaceType(OcctSurfaceType surfaceType) =>
        Faces.Where(face => face.SurfaceType == surfaceType).ToArray();
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelFaceAnalysis
{
    internal long FaceId;
    internal int SurfaceType;
    internal int Orientation;
    internal int EdgeCount;
    internal int WireCount;
    internal double Area;
    internal double MaximumTolerance;
    internal OcctUvBounds UvBounds;
    internal OcctBounds Bounds;
}
