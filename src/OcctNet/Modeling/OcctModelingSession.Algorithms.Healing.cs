namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelAlgorithmResult UnifySameDomain(
        OcctModelShape shape,
        bool unifyEdges = true,
        bool unifyFaces = true,
        bool concatenateBSplines = false)
    {
        EnsureShape(shape);
        var status = ModelNativeMethods.occt_model_healing_unify_same_domain_execute(
            _handle,
            shape.Id,
            unifyEdges ? 1 : 0,
            unifyFaces ? 1 : 0,
            concatenateBSplines ? 1 : 0,
            out var result);
        return CheckAlgorithm(status, result);
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
        var status = ModelNativeMethods.occt_model_healing_fix_shape_execute(
            _handle,
            shape.Id,
            precision,
            minTolerance,
            maxTolerance,
            out var result);
        return CheckAlgorithm(status, result);
    }
}
