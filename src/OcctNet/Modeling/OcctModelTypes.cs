using System.Runtime.InteropServices;

namespace OcctNet;

/// <summary>Describes the containment state of a point or shape relative to another shape.</summary>
public enum OcctModelState
{
    /// <summary>The containment state could not be determined.</summary>
    Unknown = 0,
    /// <summary>The point or shape is inside the reference shape.</summary>
    Inside = 1,
    /// <summary>The point or shape is outside the reference shape.</summary>
    Outside = 2,
    /// <summary>The point or shape lies on the boundary of the reference shape.</summary>
    On = 3
}

/// <summary>Orientation of a topological sub-shape within its parent shape.</summary>
public enum OcctModelOrientation
{
    /// <summary>The sub-shape is oriented in the same direction as its parent.</summary>
    Forward = 0,
    /// <summary>The sub-shape is oriented in the opposite direction to its parent.</summary>
    Reversed = 1,
    /// <summary>The sub-shape is internal (e.g. a seam edge).</summary>
    Internal = 2,
    /// <summary>The sub-shape is external.</summary>
    External = 3
}

/// <summary>
/// Glue mode hint for OCCT Boolean operations.
/// Can significantly accelerate operations on shapes that share faces.
/// </summary>
public enum OcctModelBooleanGlue
{
    /// <summary>No glue \u2014 standard Boolean algorithm (default).</summary>
    Off = 0,
    /// <summary>Shift glue \u2014 use when one shape lies fully outside the other.</summary>
    Shift = 1,
    /// <summary>Full glue \u2014 use when shapes share a full coincident face set.</summary>
    Full = 2
}

/// <summary>
/// Configuration for OCCT Boolean operations (Fuse, Cut, Common, Section, Split).
/// Use <see cref="Default"/> or <see cref="CreateDefault"/> as the starting point
/// and adjust individual properties as needed.
/// </summary>
public struct OcctModelBooleanOptions
{
    /// <summary>
    /// Fuzzy tolerance applied to coincident geometry detection.
    /// Use a small positive value (e.g. 1e-6) when shapes have near-coincident faces.
    /// Default: 0 (automatic).
    /// </summary>
    public double FuzzyValue { get; set; }

    /// <summary>
    /// Angular tolerance used when simplifying the result shape.
    /// Default: 1e-7 radians.
    /// </summary>
    public double AngularTolerance { get; set; }

    /// <summary>Enables multi-threaded OCCT Boolean execution. Default: <see langword="true"/>.</summary>
    public bool RunParallel { get; set; }

    /// <summary>
    /// Preserves the input shapes unchanged (non-destructive mode).
    /// Default: <see langword="true"/>.
    /// </summary>
    public bool NonDestructive { get; set; }

    /// <summary>Glue hint for coincident geometry. Default: <see cref="OcctModelBooleanGlue.Off"/>.</summary>
    public OcctModelBooleanGlue Glue { get; set; }

    /// <summary>Checks for inverted (inside-out) solids in the result. Default: <see langword="true"/>.</summary>
    public bool CheckInverted { get; set; }

    /// <summary>Removes redundant edges from the result shape. Default: <see langword="true"/>.</summary>
    public bool SimplifyEdges { get; set; }

    /// <summary>Merges coplanar faces in the result shape. Default: <see langword="true"/>.</summary>
    public bool SimplifyFaces { get; set; }

    /// <summary>
    /// Returns a <see cref="OcctModelBooleanOptions"/> instance initialized with recommended defaults.
    /// This property returns a value-type copy; modify the returned value freely.
    /// </summary>
    public static OcctModelBooleanOptions Default => new()
    {
        FuzzyValue = 0.0,
        AngularTolerance = 1e-7,
        RunParallel = true,
        NonDestructive = true,
        Glue = OcctModelBooleanGlue.Off,
        CheckInverted = true,
        SimplifyEdges = true,
        SimplifyFaces = true
    };

    /// <summary>
    /// Returns a new <see cref="OcctModelBooleanOptions"/> with recommended defaults.
    /// Prefer this over the <see cref="Default"/> property when the result will be
    /// mutated, to avoid accidental struct-copy pitfalls.
    /// </summary>
    public static OcctModelBooleanOptions CreateDefault() => Default;

    internal readonly NativeModelBooleanOptions ToNative() => new()
    {
        FuzzyValue = FuzzyValue,
        AngularTolerance = AngularTolerance,
        RunParallel = RunParallel ? 1 : 0,
        NonDestructive = NonDestructive ? 1 : 0,
        Glue = (int)Glue,
        CheckInverted = CheckInverted ? 1 : 0,
        SimplifyEdges = SimplifyEdges ? 1 : 0,
        SimplifyFaces = SimplifyFaces ? 1 : 0
    };
}


[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelBooleanOptions
{
    public double FuzzyValue;
    public double AngularTolerance;
    public int RunParallel;
    public int NonDestructive;
    public int Glue;
    public int CheckInverted;
    public int SimplifyEdges;
    public int SimplifyFaces;
}

/// <summary>Continuity level: None, order zero, order one, or order two.</summary>
public enum OcctContinuityLevel
{
    None = 0,
    Order0 = 1,
    Order1 = 2,
    Order2 = 3
}

/// <summary>Tolerances used for edge-to-edge parametric and geometric continuity checks.</summary>
public readonly record struct OcctCurveContinuityOptions(
    double PositionTolerance,
    double AngularTolerance,
    double CurvatureTolerance,
    double FirstDerivativeTolerance,
    double SecondDerivativeTolerance)
{
    public static OcctCurveContinuityOptions Default => new(1e-7, 1e-6, 1e-6, 1e-6, 1e-6);

    internal NativeModelContinuityOptions ToNative() => new()
    {
        PositionTolerance = PositionTolerance,
        AngularTolerance = AngularTolerance,
        CurvatureTolerance = CurvatureTolerance,
        FirstDerivativeTolerance = FirstDerivativeTolerance,
        SecondDerivativeTolerance = SecondDerivativeTolerance
    };
}

/// <summary>Measured C0-C2 and G0-G2 continuity at a selected pair of edge endpoints.</summary>
public readonly record struct OcctCurveContinuityResult(
    double PositionGap,
    double TangentAngleRadians,
    double CurvatureVectorGap,
    double FirstSpeed,
    double SecondSpeed,
    double FirstCurvature,
    double SecondCurvature,
    OcctContinuityLevel ParametricLevel,
    OcctContinuityLevel GeometricLevel,
    bool HasFirstDerivatives,
    bool HasSecondDerivatives);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelContinuityOptions
{
    public double PositionTolerance;
    public double AngularTolerance;
    public double CurvatureTolerance;
    public double FirstDerivativeTolerance;
    public double SecondDerivativeTolerance;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelCurveContinuityResult
{
    public double PositionGap;
    public double TangentAngleRadians;
    public double CurvatureVectorGap;
    public double FirstSpeed;
    public double SecondSpeed;
    public double FirstCurvature;
    public double SecondCurvature;
    public int ParametricLevel;
    public int GeometricLevel;
    public int HasFirstDerivatives;
    public int HasSecondDerivatives;

    public readonly OcctCurveContinuityResult ToManaged() => new(
        PositionGap,
        TangentAngleRadians,
        CurvatureVectorGap,
        FirstSpeed,
        SecondSpeed,
        FirstCurvature,
        SecondCurvature,
        (OcctContinuityLevel)ParametricLevel,
        (OcctContinuityLevel)GeometricLevel,
        HasFirstDerivatives != 0,
        HasSecondDerivatives != 0);
}

/// <summary>Measured continuity of two faces along a shared edge.</summary>
public readonly record struct OcctSurfaceContinuityResult(
    double MaximumPositionGap,
    double MaximumNormalAngleRadians,
    double MaximumCurvatureGap,
    OcctContinuityLevel DeclaredParametricLevel,
    OcctContinuityLevel DeclaredGeometricLevel,
    OcctContinuityLevel MeasuredGeometricLevel,
    int SampleCount,
    int InvalidSampleCount);

public readonly record struct OcctCurvatureCombSample(
    double Parameter,
    OcctPoint3d Point,
    OcctVector3d CombVector,
    double Curvature,
    bool Valid);

public readonly record struct OcctSurfaceQualityOptions(
    int USamples,
    int VSamples,
    double Resolution,
    double ZebraFrequency,
    double ZebraPhase,
    OcctVector3d ViewDirection)
{
    public static OcctSurfaceQualityOptions Default => new(
        32, 32, 1e-7, 12.0, 0.0, new OcctVector3d(0, 0, -1));

    internal NativeModelSurfaceQualityOptions ToNative() => new()
    {
        USamples = USamples,
        VSamples = VSamples,
        Resolution = Resolution,
        ZebraFrequency = ZebraFrequency,
        ZebraPhase = ZebraPhase,
        ViewDirection = ViewDirection
    };
}

public readonly record struct OcctSurfaceQualitySample(
    double U,
    double V,
    OcctPoint3d Point,
    OcctVector3d Normal,
    double MaximumCurvature,
    double MinimumCurvature,
    double MeanCurvature,
    double GaussianCurvature,
    double ZebraIntensity,
    bool Valid);

public readonly record struct OcctSurfaceQualitySummary(
    double MinimumMeanCurvature,
    double MaximumMeanCurvature,
    double MinimumGaussianCurvature,
    double MaximumGaussianCurvature,
    double MaximumAbsoluteCurvature,
    int ValidSampleCount,
    int InvalidSampleCount,
    int NormalFlipCount);

public sealed record OcctSurfaceQualityAnalysis(
    OcctSurfaceQualitySummary Summary,
    IReadOnlyList<OcctSurfaceQualitySample> Samples);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelSurfaceContinuityResult
{
    public double MaximumPositionGap;
    public double MaximumNormalAngleRadians;
    public double MaximumCurvatureGap;
    public int DeclaredParametricLevel;
    public int DeclaredGeometricLevel;
    public int MeasuredGeometricLevel;
    public int SampleCount;
    public int InvalidSampleCount;

    public readonly OcctSurfaceContinuityResult ToManaged() => new(
        MaximumPositionGap,
        MaximumNormalAngleRadians,
        MaximumCurvatureGap,
        (OcctContinuityLevel)DeclaredParametricLevel,
        (OcctContinuityLevel)DeclaredGeometricLevel,
        (OcctContinuityLevel)MeasuredGeometricLevel,
        SampleCount,
        InvalidSampleCount);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelCurvatureCombSample
{
    public double Parameter;
    public OcctPoint3d Point;
    public OcctVector3d CombVector;
    public double Curvature;
    public int Valid;

    public readonly OcctCurvatureCombSample ToManaged() =>
        new(Parameter, Point, CombVector, Curvature, Valid != 0);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelSurfaceQualityOptions
{
    public int USamples;
    public int VSamples;
    public double Resolution;
    public double ZebraFrequency;
    public double ZebraPhase;
    public OcctVector3d ViewDirection;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelSurfaceQualitySample
{
    public double U;
    public double V;
    public OcctPoint3d Point;
    public OcctVector3d Normal;
    public double MaximumCurvature;
    public double MinimumCurvature;
    public double MeanCurvature;
    public double GaussianCurvature;
    public double ZebraIntensity;
    public int Valid;

    public readonly OcctSurfaceQualitySample ToManaged() => new(
        U, V, Point, Normal, MaximumCurvature, MinimumCurvature,
        MeanCurvature, GaussianCurvature, ZebraIntensity, Valid != 0);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelSurfaceQualitySummary
{
    public double MinimumMeanCurvature;
    public double MaximumMeanCurvature;
    public double MinimumGaussianCurvature;
    public double MaximumGaussianCurvature;
    public double MaximumAbsoluteCurvature;
    public int ValidSampleCount;
    public int InvalidSampleCount;
    public int NormalFlipCount;

    public readonly OcctSurfaceQualitySummary ToManaged() => new(
        MinimumMeanCurvature,
        MaximumMeanCurvature,
        MinimumGaussianCurvature,
        MaximumGaussianCurvature,
        MaximumAbsoluteCurvature,
        ValidSampleCount,
        InvalidSampleCount,
        NormalFlipCount);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelAlgorithmResult
{
    public long ShapeId;
    public long OperationId;
    public int Succeeded;
    public int HasWarnings;
    public int HasErrors;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelProjectionResult
{
    public OcctPoint3d Point;
    public double Distance;
    public double Parameter;
    public double U;
    public double V;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelRayHit
{
    public OcctPoint3d Point;
    public long FaceId;
    public double RayParameter;
    public double U;
    public double V;
    public int State;

    public readonly OcctModelRayHit ToManaged(long ownerId) => new(
        Point,
        new OcctModelShape(FaceId, ownerId),
        RayParameter,
        U,
        V,
        (OcctModelState)State);
}

public readonly record struct OcctModelRayHit(
    OcctPoint3d Point,
    OcctModelShape Face,
    double RayParameter,
    double U,
    double V,
    OcctModelState State);

public struct OcctModelMeshParameters
{
    public double LinearDeflection { get; set; }
    public double AngularDeflection { get; set; }
    public double MinimumSize { get; set; }
    public bool Relative { get; set; }
    public bool Parallel { get; set; }
    public bool InternalVertices { get; set; }
    public bool ControlSurfaceDeflection { get; set; }

    public static OcctModelMeshParameters Default => new()
    {
        LinearDeflection = 0.1,
        AngularDeflection = 0.5,
        MinimumSize = 0.01,
        Relative = false,
        Parallel = true,
        InternalVertices = true,
        ControlSurfaceDeflection = true
    };

    internal readonly NativeModelMeshParameters ToNative() => new()
    {
        LinearDeflection = LinearDeflection,
        AngularDeflection = AngularDeflection,
        MinSize = MinimumSize,
        Relative = Relative ? 1 : 0,
        Parallel = Parallel ? 1 : 0,
        InternalVertices = InternalVertices ? 1 : 0,
        ControlSurfaceDeflection = ControlSurfaceDeflection ? 1 : 0
    };

    internal readonly NativeMeshBuildOptions ToResourceNative() => new()
    {
        StructSize = (uint)Marshal.SizeOf<NativeMeshBuildOptions>(),
        ApiVersion = 1,
        LinearDeflection = LinearDeflection,
        AngularDeflection = AngularDeflection,
        MinSize = MinimumSize,
        Relative = Relative ? 1 : 0,
        Parallel = Parallel ? 1 : 0,
        InternalVertices = InternalVertices ? 1 : 0,
        ControlSurfaceDeflection = ControlSurfaceDeflection ? 1 : 0
    };
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelMeshParameters
{
    public double LinearDeflection;
    public double AngularDeflection;
    public double MinSize;
    public int Relative;
    public int Parallel;
    public int InternalVertices;
    public int ControlSurfaceDeflection;
}

/// <summary>
/// Blittable mesh vertex layout for direct Native-to-managed buffer copies.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct OcctMeshVertex
{
    public OcctPoint3d Point;
    public double U;
    public double V;
    public OcctVector3d Normal;
    private int _hasUv;
    private int _hasNormal;

    public readonly bool HasUv => _hasUv != 0;
    public readonly bool HasNormal => _hasNormal != 0;

    internal readonly OcctModelMeshNode ToManaged() => new(
        Point,
        U,
        V,
        Normal,
        HasUv,
        HasNormal);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelMeshNode
{
    public OcctPoint3d Point;
    public double U;
    public double V;
    public OcctVector3d Normal;
    public int HasUv;
    public int HasNormal;

    public readonly OcctModelMeshNode ToManaged() => new(
        Point,
        U,
        V,
        Normal,
        HasUv != 0,
        HasNormal != 0);
}

public readonly record struct OcctModelMeshNode(
    OcctPoint3d Point,
    double U,
    double V,
    OcctVector3d Normal,
    bool HasUv,
    bool HasNormal);

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelMeshTriangle
{
    public int Node1;
    public int Node2;
    public int Node3;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelLocation
{
    public double M11;
    public double M12;
    public double M13;
    public double M14;
    public double M21;
    public double M22;
    public double M23;
    public double M24;
    public double M31;
    public double M32;
    public double M33;
    public double M34;
    public double M41;
    public double M42;
    public double M43;
    public double M44;

    public readonly bool IsFinite =>
        double.IsFinite(M11) && double.IsFinite(M12) && double.IsFinite(M13) && double.IsFinite(M14) &&
        double.IsFinite(M21) && double.IsFinite(M22) && double.IsFinite(M23) && double.IsFinite(M24) &&
        double.IsFinite(M31) && double.IsFinite(M32) && double.IsFinite(M33) && double.IsFinite(M34) &&
        double.IsFinite(M41) && double.IsFinite(M42) && double.IsFinite(M43) && double.IsFinite(M44);

    public static OcctModelLocation Identity => new()
    {
        M11 = 1,
        M22 = 1,
        M33 = 1,
        M44 = 1
    };
}

public readonly record struct OcctModelShape
{
    internal OcctModelShape(long id, long ownerId)
    {
        Id = id;
        OwnerId = ownerId;
    }

    public long Id { get; }
    internal long OwnerId { get; }
    public bool IsValid => Id > 0;
    public override string ToString() => $"ModelShape {Id}";
}

public sealed class OcctModelAlgorithmResult
{
    private readonly OcctModelingSession? _session;
    private readonly long _operationId;
    private string? _report;

    internal OcctModelAlgorithmResult(OcctModelingSession session, NativeModelAlgorithmResult native)
    {
        Shape = new OcctModelShape(native.ShapeId, session.OwnerId);
        OperationId = native.OperationId;
        Succeeded = native.Succeeded != 0;
        HasWarnings = native.HasWarnings != 0;
        HasErrors = native.HasErrors != 0;
        _operationId = native.OperationId;

        // Eagerly fetch the report when there are diagnostics, because the session
        // might be disposed before the caller accesses Report.
        if (HasWarnings || HasErrors)
        {
            _report = FetchReport(session, native.OperationId);
            _session = null; // No need to hold the session reference.
        }
        else
        {
            _session = session;
        }
    }

    public OcctModelShape Shape { get; }
    public long OperationId { get; }
    public bool Succeeded { get; }
    public bool HasWarnings { get; }
    public bool HasErrors { get; }

    /// <summary>
    /// Gets the OCCT algorithm operation report (errors and warnings).
    /// Lazily fetched on first access when there are no diagnostics;
    /// eagerly fetched during construction when warnings or errors are present.
    /// </summary>
    public string Report
    {
        get
        {
            if (_report is not null) return _report;
            if (_session is null || _session.IsDisposed) return string.Empty;
            return _report ??= FetchReport(_session, _operationId);
        }
    }

    private static string FetchReport(OcctModelingSession session, long operationId)
    {
        try
        {
            return session.GetOperationReport(operationId);
        }
        catch
        {
            // Best-effort: return empty string if the session is disposed or the report is unavailable.
            return string.Empty;
        }
    }
}

public sealed class OcctMesh
{
    internal OcctMesh(
        IReadOnlyList<OcctModelMeshNode> nodes,
        IReadOnlyList<OcctModelMeshTriangle> triangles)
    {
        Nodes = nodes;
        Triangles = triangles;
    }

    public IReadOnlyList<OcctModelMeshNode> Nodes { get; }
    public IReadOnlyList<OcctModelMeshTriangle> Triangles { get; }
}
