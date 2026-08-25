namespace OcctNet;

public sealed partial class OcctModelingSession
{
    /// <summary>
    /// Measures C0-C2 and G0-G2 continuity between selected endpoints of two edge shapes.
    /// Tangents are oriented into the first join endpoint and out of the second.
    /// </summary>
    public OcctCurveContinuityResult AnalyzeCurveContinuity(
        OcctModelShape firstEdge,
        bool firstAtEnd,
        OcctModelShape secondEdge,
        bool secondAtStart,
        OcctCurveContinuityOptions? options = null)
    {
        EnsureShape(firstEdge);
        EnsureShape(secondEdge);
        var actual = options ?? OcctCurveContinuityOptions.Default;
        ValidateContinuityOptions(actual);
        var nativeOptions = actual.ToNative();
        var status = ModelNativeMethods.occt_model_curve_continuity_analyze(
            _handle,
            firstEdge.Id,
            firstAtEnd ? 1 : 0,
            secondEdge.Id,
            secondAtStart ? 1 : 0,
            in nativeOptions,
            out var result);
        CheckStatus(status);
        return result.ToManaged();
    }

    private static void ValidateContinuityOptions(OcctCurveContinuityOptions options)
    {
        var values = new[]
        {
            options.PositionTolerance,
            options.AngularTolerance,
            options.CurvatureTolerance,
            options.FirstDerivativeTolerance,
            options.SecondDerivativeTolerance
        };
        if (values.Any(value => !double.IsFinite(value) || value < 0.0))
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Continuity tolerances must be finite and non-negative.");
    }
}
