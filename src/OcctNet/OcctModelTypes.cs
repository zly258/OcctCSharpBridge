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

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelBooleanOptions
{
    public double FuzzyValue;
    public double AngularTolerance;
    public int RunParallel;
    public int NonDestructive;
    public int Glue;
    public int CheckInverted;
    public int SimplifyEdges;
    public int SimplifyFaces;

    public bool UseParallelProcessing
    {
        readonly get => RunParallel != 0;
        set => RunParallel = value ? 1 : 0;
    }

    public bool NonDestructiveMode
    {
        readonly get => NonDestructive != 0;
        set => NonDestructive = value ? 1 : 0;
    }

    public OcctModelBooleanGlue GlueMode
    {
        readonly get => (OcctModelBooleanGlue)Glue;
        set => Glue = (int)value;
    }

    public bool CheckInvertedSolids
    {
        readonly get => CheckInverted != 0;
        set => CheckInverted = value ? 1 : 0;
    }

    public bool SimplifyResultEdges
    {
        readonly get => SimplifyEdges != 0;
        set => SimplifyEdges = value ? 1 : 0;
    }

    public bool SimplifyResultFaces
    {
        readonly get => SimplifyFaces != 0;
        set => SimplifyFaces = value ? 1 : 0;
    }

    public static OcctModelBooleanOptions Default => new()
    {
        FuzzyValue = 0.0,
        AngularTolerance = 1e-7,
        RunParallel = 1,
        NonDestructive = 1,
        Glue = (int)OcctModelBooleanGlue.Off,
        CheckInverted = 1,
        SimplifyEdges = 1,
        SimplifyFaces = 1
    };
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
    public int NativeState;

    public readonly OcctModelRayHit ToManaged(long ownerId) => new()
    {
        Point = Point,
        FaceId = FaceId,
        RayParameter = RayParameter,
        U = U,
        V = V,
        NativeState = NativeState,
        OwnerId = ownerId
    };
}

public struct OcctModelRayHit
{
    public OcctPoint3d Point;
    public long FaceId;
    public double RayParameter;
    public double U;
    public double V;
    public int NativeState;

    internal long OwnerId;

    public readonly OcctModelShape Face => new(FaceId, OwnerId);
    public readonly OcctModelState State => (OcctModelState)NativeState;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelMeshParameters
{
    public double LinearDeflection;
    public double AngularDeflection;
    public double MinSize;
    public int Relative;
    public int Parallel;
    public int InternalVertices;
    public int ControlSurfaceDeflection;

    public bool RelativeDeflection
    {
        readonly get => Relative != 0;
        set => Relative = value ? 1 : 0;
    }

    public bool UseParallelMeshing
    {
        readonly get => Parallel != 0;
        set => Parallel = value ? 1 : 0;
    }

    public bool IncludeInternalVertices
    {
        readonly get => InternalVertices != 0;
        set => InternalVertices = value ? 1 : 0;
    }

    public bool ControlSurfaceDeflectionEnabled
    {
        readonly get => ControlSurfaceDeflection != 0;
        set => ControlSurfaceDeflection = value ? 1 : 0;
    }

    public static OcctModelMeshParameters Default => new()
    {
        LinearDeflection = 0.1,
        AngularDeflection = 0.5,
        MinSize = 0.01,
        Relative = 0,
        Parallel = 1,
        InternalVertices = 1,
        ControlSurfaceDeflection = 1
    };
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelMeshNode
{
    public OcctPoint3d Point;
    public double U;
    public double V;
    public OcctVector3d Normal;
    public int NativeHasUv;
    public int NativeHasNormal;

    public readonly bool HasUv => NativeHasUv != 0;
    public readonly bool HasNormal => NativeHasNormal != 0;
}

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
    public OcctModelShape(long id)
    {
        Id = id;
        OwnerId = 0;
    }

    internal OcctModelShape(long id, long ownerId)
    {
        Id = id;
        OwnerId = ownerId;
    }

    public long Id { get; }
    internal long OwnerId { get; }

    public bool IsValid => Id > 0;
    public bool IsBound => OwnerId != 0;
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

public sealed class OcctFaceMesh
{
    internal OcctFaceMesh(IReadOnlyList<OcctModelMeshNode> nodes, IReadOnlyList<OcctModelMeshTriangle> triangles)
    {
        Nodes = nodes;
        Triangles = triangles;
    }

    public IReadOnlyList<OcctModelMeshNode> Nodes { get; }
    public IReadOnlyList<OcctModelMeshTriangle> Triangles { get; }
}
