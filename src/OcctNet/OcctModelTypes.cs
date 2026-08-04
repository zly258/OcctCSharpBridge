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
public struct OcctModelRayHit
{
    public OcctPoint3d Point;
    public long FaceId;
    public double RayParameter;
    public double U;
    public double V;
    public int NativeState;

    public readonly OcctModelShape Face => new(FaceId);
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

public readonly record struct OcctModelShape(long Id)
{
    public bool IsValid => Id > 0;
    public override string ToString() => $"ModelShape {Id}";
}

public sealed class OcctModelAlgorithmResult
{
    internal OcctModelAlgorithmResult(OcctModelingSession session, NativeModelAlgorithmResult native)
    {
        Session = session;
        Shape = new OcctModelShape(native.ShapeId);
        OperationId = native.OperationId;
        Succeeded = native.Succeeded != 0;
        HasWarnings = native.HasWarnings != 0;
        HasErrors = native.HasErrors != 0;
    }

    internal OcctModelingSession Session { get; }
    public OcctModelShape Shape { get; }
    public long OperationId { get; }
    public bool Succeeded { get; }
    public bool HasWarnings { get; }
    public bool HasErrors { get; }
    public string Report => Session.GetOperationReport(OperationId);
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
