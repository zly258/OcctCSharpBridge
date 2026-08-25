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
        var nativeOptions = actual.ToNative();
        var status = ModelNativeMethods.occt_model_boolean_execute(
            _handle,
            (int)operation,
            left.Id,
            right.Id,
            in nativeOptions,
            out var result);
        return CheckAlgorithm(status, result);
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

    /// <summary>Runs OCCT General Fuse for all supplied arguments and returns every split part.</summary>
    public OcctModelAlgorithmResult GeneralFuse(
        IEnumerable<OcctModelShape> shapes,
        OcctModelBooleanOptions? options = null)
    {
        var shapeIds = ShapeIds(shapes);
        if (shapeIds.Length < 2)
            throw new ArgumentException("General Fuse requires at least two shapes.", nameof(shapes));
        var actual = AdvancedBooleanOptions(options, nameof(options));
        var nativeOptions = actual.ToNative();
        var status = ModelNativeMethods.occt_model_boolean_general_fuse_execute(
            _handle,
            shapeIds,
            shapeIds.Length,
            in nativeOptions,
            out var result);
        return CheckAlgorithm(status, result);
    }

    /// <summary>Selects CellsBuilder regions that are inside every take shape and outside every avoid shape.</summary>
    public OcctModelAlgorithmResult BuildCells(
        IEnumerable<OcctModelShape> arguments,
        IEnumerable<OcctModelShape> take,
        IEnumerable<OcctModelShape>? avoid = null,
        int material = 0,
        bool removeInternalBoundaries = false,
        OcctModelBooleanOptions? options = null)
    {
        var argumentIds = ShapeIds(arguments);
        if (argumentIds.Length < 2)
            throw new ArgumentException("CellsBuilder requires at least two argument shapes.", nameof(arguments));
        var takeIds = ShapeIds(take);
        var avoidIds = OptionalShapeIds(avoid);
        if (material < 0) throw new ArgumentOutOfRangeException(nameof(material));
        if (removeInternalBoundaries && material == 0)
            throw new ArgumentException(
                "Removing CellsBuilder internal boundaries requires a non-zero material.",
                nameof(material));

        var argumentSet = argumentIds.ToHashSet();
        if (takeIds.Any(id => !argumentSet.Contains(id)))
            throw new ArgumentException("Every take shape must also be a CellsBuilder argument.", nameof(take));
        if (avoidIds.Any(id => !argumentSet.Contains(id)))
            throw new ArgumentException("Every avoid shape must also be a CellsBuilder argument.", nameof(avoid));

        var actual = AdvancedBooleanOptions(options, nameof(options));
        var nativeOptions = actual.ToNative();
        var status = ModelNativeMethods.occt_model_boolean_cells_execute(
            _handle,
            argumentIds,
            argumentIds.Length,
            takeIds,
            takeIds.Length,
            avoidIds,
            avoidIds.Length,
            material,
            removeInternalBoundaries ? 1 : 0,
            in nativeOptions,
            out var result);
        return CheckAlgorithm(status, result);
    }

    public OcctModelAlgorithmResult Split(
        IEnumerable<OcctModelShape> objects,
        IEnumerable<OcctModelShape> tools,
        OcctModelBooleanOptions? options = null)
    {
        var objectIds = ShapeIds(objects);
        var toolIds = ShapeIds(tools);
        var actual = options ?? OcctModelBooleanOptions.Default;
        ValidateBooleanOptions(actual, nameof(options));
        var nativeOptions = actual.ToNative();
        var status = ModelNativeMethods.occt_model_boolean_split_execute(
            _handle,
            objectIds,
            objectIds.Length,
            toolIds,
            toolIds.Length,
            in nativeOptions,
            out var result);
        return CheckAlgorithm(status, result);
    }

    private long[] OptionalShapeIds(IEnumerable<OcctModelShape>? shapes)
    {
        if (shapes is null) return Array.Empty<long>();
        var values = shapes.ToArray();
        var result = new long[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            EnsureShape(values[index]);
            result[index] = values[index].Id;
        }
        return result;
    }

    private static OcctModelBooleanOptions AdvancedBooleanOptions(
        OcctModelBooleanOptions? options,
        string parameterName)
    {
        var actual = options ?? OcctModelBooleanOptions.Default;
        if (options.HasValue && (actual.SimplifyEdges || actual.SimplifyFaces))
            throw new NotSupportedException(
                "BOPAlgo General Fuse and CellsBuilder do not expose BRepAlgoAPI result simplification.");
        actual.SimplifyEdges = false;
        actual.SimplifyFaces = false;
        ValidateBooleanOptions(actual, parameterName);
        return actual;
    }

    private static void ValidateBooleanOptions(OcctModelBooleanOptions options, string parameterName)
    {
        OcctGuard.NonNegative(options.FuzzyValue, $"{parameterName}.{nameof(options.FuzzyValue)}");
        OcctGuard.Positive(options.AngularTolerance, $"{parameterName}.{nameof(options.AngularTolerance)}");
        if (!Enum.IsDefined(options.Glue))
            throw new ArgumentOutOfRangeException(parameterName, options.Glue, "Boolean glue mode is invalid.");
    }
}
