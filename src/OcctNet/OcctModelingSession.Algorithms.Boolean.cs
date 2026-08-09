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

    private static void ValidateBooleanOptions(OcctModelBooleanOptions options, string parameterName)
    {
        OcctGuard.NonNegative(options.FuzzyValue, $"{parameterName}.{nameof(options.FuzzyValue)}");
        OcctGuard.Positive(options.AngularTolerance, $"{parameterName}.{nameof(options.AngularTolerance)}");
        if (!Enum.IsDefined(options.Glue))
            throw new ArgumentOutOfRangeException(parameterName, options.Glue, "Boolean glue mode is invalid.");
    }
}
