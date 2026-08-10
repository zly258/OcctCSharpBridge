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
Assert(Math.Abs(vector.LengthSquared - 25) < 1e-12, "Vector squared-length regression.");
var normalized = vector.Normalized();
Assert(Math.Abs(normalized.Length - 1) < 1e-12, "Vector normalization regression.");
Assert(vector.TryNormalize(out var normalizedViaTry) && normalizedViaTry == normalized, "Vector TryNormalize regression.");
Assert(!OcctVector3d.Zero.TryNormalize(out _), "Zero vector must not normalize.");
Assert(OcctVector3d.UnitX.Cross(OcctVector3d.UnitY) == OcctVector3d.UnitZ, "Vector equality/cross product regression.");
Assert(OcctVector3d.UnitX + OcctVector3d.UnitY == new OcctVector3d(1, 1, 0), "Vector addition regression.");
Assert(2 * OcctVector3d.UnitX == new OcctVector3d(2, 0, 0), "Left scalar multiplication regression.");

var point = new OcctPoint3d(1, 2, 3);
Assert(point + OcctVector3d.UnitX == new OcctPoint3d(2, 2, 3), "Point translation regression.");
Assert(point - OcctVector3d.UnitZ == new OcctPoint3d(1, 2, 2), "Point subtraction regression.");
Assert(Math.Abs(point.DistanceTo(new OcctPoint3d(1, 2, 8)) - 5) < 1e-12, "Point distance regression.");
Assert(point.IsFinite && !new OcctPoint3d(double.NaN, 0, 0).IsFinite, "Point finite-state regression.");

var booleanOptions = OcctModelBooleanOptions.Default;
Assert(booleanOptions.RunParallel, "Boolean parallel option regression.");
Assert(booleanOptions.NonDestructive, "Boolean non-destructive option regression.");
booleanOptions.RunParallel = false;
booleanOptions.Glue = OcctModelBooleanGlue.Full;
var nativeBooleanOptions = booleanOptions.ToNative();
Assert(nativeBooleanOptions.RunParallel == 0, "Boolean bool-to-native mapping regression.");
Assert(nativeBooleanOptions.Glue == (int)OcctModelBooleanGlue.Full, "Boolean enum-to-native mapping regression.");

var meshOptions = OcctModelMeshParameters.Default;
Assert(meshOptions.Parallel, "Mesh parallel option regression.");
meshOptions.Relative = true;
var nativeMeshOptions = meshOptions.ToNative();
Assert(nativeMeshOptions.Relative == 1, "Mesh bool-to-native mapping regression.");
Assert(Math.Abs(nativeMeshOptions.MinSize - meshOptions.MinimumSize) < 1e-12, "Mesh minimum-size mapping regression.");

var modelShapeA = new OcctModelShape(7, 1001);
var modelShapeB = new OcctModelShape(7, 1002);
Assert(modelShapeA.IsValid && modelShapeB.IsValid, "Model shape validity regression.");
Assert(modelShapeA.OwnerId != modelShapeB.OwnerId, "Model shape owner identity regression.");
Assert(!default(OcctModelShape).IsValid, "Default model shape must be invalid.");

var viewerShapeA = new OcctShape(3, 2001);
var viewerShapeB = new OcctShape(3, 2002);
Assert(viewerShapeA.IsValid && viewerShapeB.IsValid, "Viewer shape validity regression.");
Assert(viewerShapeA.OwnerId != viewerShapeB.OwnerId, "Viewer shape owner identity regression.");
Assert(viewerShapeA.Kind == OcctObjectKind.Shape, "Viewer shape kind regression.");
Assert(!default(OcctShape).IsValid, "Default viewer shape must be invalid.");
Assert(new OcctText(5, 2001).Kind == OcctObjectKind.Text, "Text object kind regression.");
Assert(new OcctDimension(6, 2001).Kind == OcctObjectKind.Dimension, "Dimension object kind regression.");

var transform = OcctTransform3d.Translation(1, 2, 3);
Assert(transform.IsFinite, "Transform finite-state regression.");
Assert(!new OcctTransform3d(double.NaN, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0).IsFinite, "Invalid transform finite-state regression.");
Assert(OcctModelLocation.Identity.IsFinite, "Model location finite-state regression.");
var invalidLocation = OcctModelLocation.Identity;
invalidLocation.M14 = double.PositiveInfinity;
Assert(!invalidLocation.IsFinite, "Invalid model location finite-state regression.");

var nativeHit = new NativeModelRayHit
{
    FaceId = 11,
    State = (int)OcctModelState.On,
    RayParameter = 2.5
};
var managedHit = nativeHit.ToManaged(1001);
Assert(managedHit.Face.IsValid, "Ray-hit face validity regression.");
Assert(managedHit.Face.OwnerId == 1001, "Ray-hit owner propagation regression.");
Assert(managedHit.State == OcctModelState.On, "Ray-hit state mapping regression.");

var nativeInertia = new NativeModelInertiaProperties
{
    Mass = 12.5,
    CenterOfMass = new OcctPoint3d(1, 2, 3),
    Ixx = 4,
    Iyy = 5,
    Izz = 6,
    PrincipalMoment1 = 7,
    PrincipalMoment2 = 8,
    PrincipalMoment3 = 9,
    PrincipalAxis1 = OcctVector3d.UnitX,
    PrincipalAxis2 = OcctVector3d.UnitY,
    PrincipalAxis3 = OcctVector3d.UnitZ,
    HasSymmetryAxis = 1,
    HasSymmetryPoint = 0
};
var inertia = nativeInertia.ToManaged();
Assert(inertia.Mass == 12.5 && inertia.CenterOfMass == new OcctPoint3d(1, 2, 3), "Inertia mapping regression.");
Assert(inertia.HasSymmetryAxis && !inertia.HasSymmetryPoint, "Inertia symmetry mapping regression.");

var nativeIntersection = new NativeModelEdgeIntersection
{
    Kind = (int)OcctIntersectionKind.Overlap,
    StartPoint = new OcctPoint3d(0, 0, 0),
    EndPoint = new OcctPoint3d(5, 0, 0),
    FirstParameterStart = 1,
    FirstParameterEnd = 2,
    SecondParameterStart = 3,
    SecondParameterEnd = 4
};
var intersection = nativeIntersection.ToManaged();
Assert(intersection.Kind == OcctIntersectionKind.Overlap, "Intersection kind mapping regression.");
Assert(intersection.FirstParameterStart == 1 && intersection.SecondParameterEnd == 4, "Intersection parameter mapping regression.");

var topologyBounds = new OcctBounds
{
    MinX = 0,
    MinY = 0,
    MinZ = 0,
    MaxX = 10,
    MaxY = 0,
    MaxZ = 0
};
var topologyReference = new OcctTopologyReference(
    1,
    OcctShapeType.Edge,
    3,
    OcctCurveType.Line,
    OcctSurfaceType.Other,
    10,
    new OcctPoint3d(5, 0, 0),
    topologyBounds,
    1e-7,
    OcctModelOrientation.Forward,
    2,
    0,
    1);
var nativeReference = NativeModelTopologyReference.FromManaged(topologyReference);
var roundTripReference = nativeReference.ToManaged();
Assert(roundTripReference == topologyReference, "Topology-reference mapping regression.");

var nativeReferenceResult = new NativeModelTopologyReferenceResult
{
    Status = (int)OcctTopologyReferenceStatus.Resolved,
    ShapeId = 42,
    Score = 0.91,
    CandidateCount = 2,
    UsedOperationHistory = 1,
    RuntimeIndexMatched = 0
};
var referenceResult = nativeReferenceResult.ToManaged(1001);
Assert(referenceResult.Status == OcctTopologyReferenceStatus.Resolved, "Topology-reference result status regression.");
Assert(
    referenceResult.Shape.HasValue &&
    referenceResult.Shape.Value.IsValid &&
    referenceResult.Shape.Value.OwnerId == 1001,
    "Topology-reference result owner regression.");
Assert(referenceResult.UsedOperationHistory && !referenceResult.RuntimeIndexMatched, "Topology-reference result flag regression.");

var orientedBounds = new OcctOrientedBounds
{
    Center = OcctPoint3d.Origin,
    XDirection = OcctVector3d.UnitX,
    YDirection = OcctVector3d.UnitY,
    ZDirection = OcctVector3d.UnitZ,
    HalfSizeX = 1,
    HalfSizeY = 2,
    HalfSizeZ = 3
};
Assert(orientedBounds.IsFinite, "Oriented bounds finite-state regression.");
Assert(orientedBounds.SizeX == 2 && orientedBounds.SizeY == 4 && orientedBounds.SizeZ == 6, "Oriented bounds size regression.");
Assert(orientedBounds.Volume == 48, "Oriented bounds volume regression.");
Assert(Enum.IsDefined(OcctJoinType.Intersection), "Join-type enum regression.");

var resolvedSelectionEnd = OcctViewportInteractionPolicy.ResolveSelectionEnd(0, 0, 5, 5, 20, 10, rectangleDragStarted: true);
Assert(resolvedSelectionEnd == (20, 10), "Viewport rectangle end recovery regression.");
Assert(OcctViewportInteractionPolicy.ShouldUseRectangle(true, false, 3, 0, 0, 4, 0), "Viewport rectangle threshold regression.");
Assert(!OcctViewportInteractionPolicy.ShouldUseRectangle(true, false, 3, 0, 0, 1, 1), "Viewport click threshold regression.");
Assert(OcctViewportInteractionPolicy.AllowsOverlap(OcctRectangleSelectionBehavior.Directional, 20, 5), "Directional overlap regression.");
Assert(!OcctViewportInteractionPolicy.AllowsOverlap(OcctRectangleSelectionBehavior.Directional, 5, 20), "Directional inclusive regression.");
Assert(Math.Abs(OcctViewportInteractionPolicy.ZoomFactor(120) - 1.15) < 1e-12, "Viewport zoom-in policy regression.");
Assert(Math.Abs(OcctViewportInteractionPolicy.ZoomFactor(-120) - 0.87) < 1e-12, "Viewport zoom-out policy regression.");

Expect<ArgumentOutOfRangeException>(() => OcctGuard.Positive(0, "value"), "Positive guard accepted zero.");
Expect<ArgumentOutOfRangeException>(() => OcctGuard.UnitInterval(1.1, "value"), "Unit interval guard accepted value above one.");
Expect<ArgumentException>(() => OcctGuard.NonZero(OcctVector3d.Zero, "vector"), "Non-zero vector guard accepted zero vector.");
Expect<ArgumentException>(() => OcctGuard.Finite(new OcctPoint3d(double.NaN, 0, 0), "point"), "Finite point guard accepted NaN.");
Expect<ArgumentOutOfRangeException>(() => OcctGuard.AtLeast(2, 3, "value"), "Minimum-value guard accepted a value below minimum.");

var exception = new OcctException("managed message", "Cut", "native message");
Assert(exception.Operation == "Cut", "OcctException operation metadata regression.");
Assert(exception.NativeMessage == "native message", "OcctException native message regression.");

var impossibleDirectory = Path.Combine(Path.GetTempPath(), $"occt-missing-{Guid.NewGuid():N}");
Expect<DirectoryNotFoundException>(
    () => OcctRuntime.Configure(new OcctRuntimeOptions { NativeBridgeDirectory = impossibleDirectory }),
    "Runtime configuration accepted a missing explicit native directory.");

Console.WriteLine("Managed bridge regression tests passed.");
