using OcctNet;

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void Expect<TException>(Action action, string message)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException(message);
}

var vector = new OcctVector3d(3, 4, 0);
Assert(Math.Abs(vector.Length - 5) < 1e-12, "Vector length regression.");
var normalized = vector.Normalized();
Assert(Math.Abs(normalized.Length - 1) < 1e-12, "Vector normalization regression.");
Assert(OcctVector3d.UnitX.Cross(OcctVector3d.UnitY) == OcctVector3d.UnitZ, "Vector cross product regression.");

var booleanOptions = OcctModelBooleanOptions.Default;
Assert(booleanOptions.UseParallelProcessing, "Boolean parallel option mapping regression.");
Assert(booleanOptions.NonDestructiveMode, "Boolean non-destructive option mapping regression.");
booleanOptions.UseParallelProcessing = false;
booleanOptions.GlueMode = OcctModelBooleanGlue.Full;
Assert(booleanOptions.RunParallel == 0, "Boolean bool-to-native mapping regression.");
Assert(booleanOptions.Glue == (int)OcctModelBooleanGlue.Full, "Boolean enum-to-native mapping regression.");

var meshOptions = OcctModelMeshParameters.Default;
Assert(meshOptions.UseParallelMeshing, "Mesh parallel option mapping regression.");
meshOptions.RelativeDeflection = true;
Assert(meshOptions.Relative == 1, "Mesh bool-to-native mapping regression.");

var modelShapeA = new OcctModelShape(7, 1001);
var modelShapeB = new OcctModelShape(7, 1002);
Assert(modelShapeA.IsBound && modelShapeB.IsBound, "Model shape owner binding regression.");
Assert(modelShapeA.OwnerId != modelShapeB.OwnerId, "Model shape owner identity regression.");
Assert(new OcctModelShape(7).IsBound == false, "Legacy model shape must remain unbound.");

var viewerShapeA = new OcctShape(3, 2001);
var viewerShapeB = new OcctShape(3, 2002);
Assert(viewerShapeA.IsBound && viewerShapeB.IsBound, "Viewer shape owner binding regression.");
Assert(viewerShapeA.OwnerId != viewerShapeB.OwnerId, "Viewer shape owner identity regression.");
Assert(new OcctShape(3).IsBound == false, "Legacy viewer shape must remain unbound.");

var nativeHit = new NativeModelRayHit
{
    FaceId = 11,
    NativeState = (int)OcctModelState.On,
    RayParameter = 2.5
};
var managedHit = nativeHit.ToManaged(1001);
Assert(managedHit.Face.IsBound, "Ray-hit face must inherit modeling-session ownership.");
Assert(managedHit.Face.OwnerId == 1001, "Ray-hit owner propagation regression.");
Assert(managedHit.State == OcctModelState.On, "Ray-hit state mapping regression.");

Expect<ArgumentOutOfRangeException>(() => OcctGuard.Positive(0, "value"), "Positive guard accepted zero.");
Expect<ArgumentOutOfRangeException>(() => OcctGuard.UnitInterval(1.1, "value"), "Unit interval guard accepted value above one.");
Expect<ArgumentException>(() => OcctGuard.NonZero(new OcctVector3d(0, 0, 0), "vector"), "Non-zero vector guard accepted zero vector.");

var exception = new OcctException("managed message", "Cut", "native message");
Assert(exception.Operation == "Cut", "OcctException operation metadata regression.");
Assert(exception.NativeMessage == "native message", "OcctException native message regression.");

var impossibleDirectory = Path.Combine(Path.GetTempPath(), $"occt-missing-{Guid.NewGuid():N}");
Expect<DirectoryNotFoundException>(
    () => OcctRuntime.Configure(new OcctRuntimeOptions { NativeBridgeDirectory = impossibleDirectory }),
    "Runtime configuration accepted a missing explicit native directory.");

Console.WriteLine("Managed bridge regression tests passed.");
