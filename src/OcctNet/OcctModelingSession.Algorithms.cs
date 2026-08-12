namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelAlgorithmResult Boolean(
        OcctBooleanOperation operation,
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null)
    {
        EnsureShape(left);
        EnsureShape(right);
        var actual = options ?? OcctModelBooleanOptions.Default;
        return CheckAlgorithm(ModelNativeMethods.occt_model_boolean(_handle, (int)operation, left.Id, right.Id, in actual));
    }

    public OcctModelAlgorithmResult Fuse(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null) =>
        Boolean(OcctBooleanOperation.Fuse, left, right, options);

    public OcctModelAlgorithmResult Cut(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null) =>
        Boolean(OcctBooleanOperation.Cut, left, right, options);

    public OcctModelAlgorithmResult Common(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null) =>
        Boolean(OcctBooleanOperation.Common, left, right, options);

    public OcctModelAlgorithmResult Section(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null) =>
        Boolean(OcctBooleanOperation.Section, left, right, options);

    public OcctModelAlgorithmResult Split(
        IEnumerable<OcctModelShape> objects,
        IEnumerable<OcctModelShape> tools,
        OcctModelBooleanOptions? options = null)
    {
        var objectIds = ShapeIds(objects);
        var toolIds = ShapeIds(tools);
        var actual = options ?? OcctModelBooleanOptions.Default;
        return CheckAlgorithm(ModelNativeMethods.occt_model_split(_handle, objectIds, objectIds.Length, toolIds, toolIds.Length, in actual));
    }

    public OcctModelAlgorithmResult Extrude(OcctModelShape profile, OcctVector3d vector)
    {
        EnsureShape(profile);
        return CheckAlgorithm(ModelNativeMethods.occt_model_extrude(_handle, profile.Id, vector));
    }

    public OcctModelAlgorithmResult Revolve(OcctModelShape profile, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees = 360)
    {
        EnsureShape(profile);
        return CheckAlgorithm(ModelNativeMethods.occt_model_revolve(_handle, profile.Id, axisPoint, axisDirection, angleDegrees));
    }

    public OcctModelAlgorithmResult Sweep(OcctModelShape spineWire, OcctModelShape profile)
    {
        EnsureShape(spineWire);
        EnsureShape(profile);
        return CheckAlgorithm(ModelNativeMethods.occt_model_sweep(_handle, spineWire.Id, profile.Id));
    }

    public OcctModelAlgorithmResult Loft(
        IEnumerable<OcctModelShape> sectionWires,
        bool makeSolid = true,
        bool ruled = false,
        double tolerance = 1e-6)
    {
        var ids = ShapeIds(sectionWires);
        return CheckAlgorithm(ModelNativeMethods.occt_model_loft(
            _handle, ids, ids.Length, makeSolid ? 1 : 0, ruled ? 1 : 0, tolerance));
    }

    public OcctModelAlgorithmResult FilletEdges(OcctModelShape shape, IEnumerable<int> edgeIndices, double radius)
    {
        EnsureShape(shape);
        var indices = RequiredArray(edgeIndices, nameof(edgeIndices)).Distinct().ToArray();
        return CheckAlgorithm(ModelNativeMethods.occt_model_fillet_edges(_handle, shape.Id, indices, indices.Length, radius));
    }

    public OcctModelAlgorithmResult ChamferEdges(OcctModelShape shape, IEnumerable<int> edgeIndices, double distance)
    {
        EnsureShape(shape);
        var indices = RequiredArray(edgeIndices, nameof(edgeIndices)).Distinct().ToArray();
        return CheckAlgorithm(ModelNativeMethods.occt_model_chamfer_edges(_handle, shape.Id, indices, indices.Length, distance));
    }

    public OcctModelAlgorithmResult Offset(OcctModelShape shape, double offset, double tolerance = 1e-4)
    {
        EnsureShape(shape);
        return CheckAlgorithm(ModelNativeMethods.occt_model_offset(_handle, shape.Id, offset, tolerance));
    }

    public OcctModelAlgorithmResult MakeThickSolid(
        OcctModelShape solid,
        IEnumerable<int> faceIndicesToRemove,
        double thickness,
        double tolerance = 1e-4)
    {
        EnsureShape(solid);
        var indices = RequiredArray(faceIndicesToRemove, nameof(faceIndicesToRemove)).Distinct().ToArray();
        return CheckAlgorithm(ModelNativeMethods.occt_model_thick_solid(_handle, solid.Id, indices, indices.Length, thickness, tolerance));
    }

    public OcctModelAlgorithmResult UnifySameDomain(
        OcctModelShape shape,
        bool unifyEdges = true,
        bool unifyFaces = true,
        bool concatenateBSplines = false)
    {
        EnsureShape(shape);
        return CheckAlgorithm(ModelNativeMethods.occt_model_unify_same_domain(
            _handle,
            shape.Id,
            unifyEdges ? 1 : 0,
            unifyFaces ? 1 : 0,
            concatenateBSplines ? 1 : 0));
    }

    public OcctModelAlgorithmResult FixShape(
        OcctModelShape shape,
        double precision = 1e-7,
        double minTolerance = 1e-7,
        double maxTolerance = 1.0)
    {
        EnsureShape(shape);
        return CheckAlgorithm(ModelNativeMethods.occt_model_fix_shape(_handle, shape.Id, precision, minTolerance, maxTolerance));
    }
}
