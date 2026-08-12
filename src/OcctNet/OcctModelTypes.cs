using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctModelState
{
    Unknown = 0,
    Inside = 1,
    Outside = 2,
    On = 3
}

public enum OcctModelOrientation
{
    Forward = 0,
    Reversed = 1,
    Internal = 2,
    External = 3
}

public enum OcctModelBooleanGlue
{
    Off = 0,
    Shift = 1,
    Full = 2
}

public struct OcctModelBooleanOptions
{
    public double FuzzyValue { get; set; }
    public double AngularTolerance { get; set; }
    public bool RunParallel { get; set; }
    public bool NonDestructive { get; set; }
    public OcctModelBooleanGlue Glue { get; set; }
    public bool CheckInverted { get; set; }
    public bool SimplifyEdges { get; set; }
    public bool SimplifyFaces { get; set; }

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
    private readonly string _report;

    internal OcctModelAlgorithmResult(OcctModelingSession session, NativeModelAlgorithmResult native)
    {
        Shape = new OcctModelShape(native.ShapeId, session.OwnerId);
        OperationId = native.OperationId;
        Succeeded = native.Succeeded != 0;
        HasWarnings = native.HasWarnings != 0;
        HasErrors = native.HasErrors != 0;
        _report = session.GetOperationReport(native.OperationId);
    }

    public OcctModelShape Shape { get; }
    public long OperationId { get; }
    public bool Succeeded { get; }
    public bool HasWarnings { get; }
    public bool HasErrors { get; }
    public string Report => _report;
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
