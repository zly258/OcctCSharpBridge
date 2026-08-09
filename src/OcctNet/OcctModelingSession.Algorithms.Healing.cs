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
}
