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
        if (!Enum.IsDefined(operation)) throw new ArgumentOutOfRangeException(nameof(operation));
        var actual = options ?? OcctModelBooleanOptions.Default;
        ValidateBooleanOptions(actual, nameof(options));
        var native = actual.ToNative();
        return CheckAlgorithm(ModelNativeMethods.occt_model_boolean(
            _handle,
            (int)operation,
            left.Id,
            right.Id,
            in native));
    }

    public OcctModelAlgorithmResult Fuse(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null) =>
        Boolean(OcctBooleanOperation.Fuse, left, right, options);

    public OcctModelAlgorithmResult Cut(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null) =>
        Boolean(OcctBooleanOperation.Cut, left, right, options);

    public OcctModelAlgorithmResult Common(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null) =>
        Boolean(OcctBooleanOperation.Common, left, right, options);

    public OcctModelAlgorithmResult Section(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null) =>
        Boolean(OcctBooleanOperation.Section, left, right, options);

    public OcctModelAlgorithmResult Split(
        IEnumerable<OcctModelShape> objects,
        IEnumerable<OcctModelShape> tools,
        OcctModelBooleanOptions? options = null)
    {
        var objectIds = ShapeIds(objects);
        var toolIds = ShapeIds(tools);
        var actual = options ?? OcctModelBooleanOptions.Default;
        ValidateBooleanOptions(actual, nameof(options));
        var native = actual.ToNative();
        return CheckAlgorithm(ModelNativeMethods.occt_model_split(
            _handle,
            objectIds,
            objectIds.Length,
            toolIds,
            toolIds.Length,
            in native));
    }

    public OcctModelAlgorithmResult Extrude(OcctModelShape profile, OcctVector3d vector)
    {
        EnsureShape(profile);
        OcctGuard.NonZero(vector, nameof(vector));
        return CheckAlgorithm(ModelNativeMethods.occt_model_extrude(_handle, profile.Id, vector));
    }

    public OcctModelAlgorithmResult Revolve(
        OcctModelShape profile,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees = 360)
    {
        EnsureShape(profile);
        OcctGuard.Finite(axisPoint, nameof(axisPoint));
        OcctGuard.NonZero(axisDirection, nameof(axisDirection));
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        if (Math.Abs(angleDegrees) <= double.Epsilon)
            throw new ArgumentOutOfRangeException(nameof(angleDegrees), angleDegrees, "Revolve angle must be non-zero.");
        return CheckAlgorithm(ModelNativeMethods.occt_model_revolve(
            _handle,
            profile.Id,
            axisPoint,
            axisDirection,
            angleDegrees));
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
        if (ids.Length < 2)
            throw new ArgumentException("Loft requires at least two section wires.", nameof(sectionWires));
        OcctGuard.Positive(tolerance, nameof(tolerance));
        return CheckAlgorithm(ModelNativeMethods.occt_model_loft(
            _handle,
            ids,
            ids.Length,
            makeSolid ? 1 : 0,
            ruled ? 1 : 0,
            tolerance));
    }

    public OcctModelAlgorithmResult FilletEdges(
        OcctModelShape shape,
        IEnumerable<int> edgeIndices,
        double radius)
    {
        EnsureShape(shape);
        OcctGuard.Positive(radius, nameof(radius));
        var indices = RequiredArray(edgeIndices, nameof(edgeIndices)).Distinct().ToArray();
        foreach (var index in indices) OcctGuard.PositiveIndex(index, nameof(edgeIndices));
        return CheckAlgorithm(ModelNativeMethods.occt_model_fillet_edges(
            _handle,
            shape.Id,
            indices,
            indices.Length,
            radius));
    }

    public OcctModelAlgorithmResult ChamferEdges(
        OcctModelShape shape,
        IEnumerable<int> edgeIndices,
        double distance)
    {
        EnsureShape(shape);
        OcctGuard.Positive(distance, nameof(distance));
        var indices = RequiredArray(edgeIndices, nameof(edgeIndices)).Distinct().ToArray();
        foreach (var index in indices) OcctGuard.PositiveIndex(index, nameof(edgeIndices));
        return CheckAlgorithm(ModelNativeMethods.occt_model_chamfer_edges(
            _handle,
            shape.Id,
            indices,
            indices.Length,
            distance));
    }

    public OcctModelAlgorithmResult OffsetShape(
        OcctModelShape shape,
        double offset,
        double tolerance = 1e-4)
    {
        EnsureShape(shape);
        OcctGuard.Finite(offset, nameof(offset));
        if (Math.Abs(offset) <= 1e-15)
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be non-zero.");
        OcctGuard.Positive(tolerance, nameof(tolerance));
        return CheckAlgorithm(ModelNativeMethods.occt_model_offset(
            _handle,
            shape.Id,
            offset,
            tolerance));
    }

    public OcctModelAlgorithmResult MakeThickSolid(
        OcctModelShape solid,
        IEnumerable<int> faceIndicesToRemove,
        double thickness,
        double tolerance = 1e-4)
    {
        EnsureShape(solid);
        OcctGuard.Finite(thickness, nameof(thickness));
        if (Math.Abs(thickness) <= double.Epsilon)
            throw new ArgumentOutOfRangeException(nameof(thickness), thickness, "Thickness must be non-zero.");
        OcctGuard.Positive(tolerance, nameof(tolerance));
        var indices = RequiredArray(faceIndicesToRemove, nameof(faceIndicesToRemove)).Distinct().ToArray();
        foreach (var index in indices) OcctGuard.PositiveIndex(index, nameof(faceIndicesToRemove));
        return CheckAlgorithm(ModelNativeMethods.occt_model_thick_solid(
            _handle,
            solid.Id,
            indices,
            indices.Length,
            thickness,
            tolerance));
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
        OcctGuard.Positive(precision, nameof(precision));
        OcctGuard.NonNegative(minTolerance, nameof(minTolerance));
        OcctGuard.Positive(maxTolerance, nameof(maxTolerance));
        if (minTolerance > maxTolerance)
            throw new ArgumentException("minTolerance must not exceed maxTolerance.", nameof(minTolerance));
        return CheckAlgorithm(ModelNativeMethods.occt_model_fix_shape(
            _handle,
            shape.Id,
            precision,
            minTolerance,
            maxTolerance));
    }

    private static void ValidateBooleanOptions(OcctModelBooleanOptions options, string parameterName)
    {
        OcctGuard.NonNegative(options.FuzzyValue, $"{parameterName}.{nameof(options.FuzzyValue)}");
        OcctGuard.Positive(options.AngularTolerance, $"{parameterName}.{nameof(options.AngularTolerance)}");
        if (!Enum.IsDefined(options.Glue))
            throw new ArgumentOutOfRangeException(parameterName, options.Glue, "Boolean glue mode is invalid.");
    }
}
