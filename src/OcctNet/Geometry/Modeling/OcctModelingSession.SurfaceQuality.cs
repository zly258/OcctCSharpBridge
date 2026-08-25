namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctSurfaceContinuityResult AnalyzeSurfaceContinuity(
        OcctModelShape firstFace,
        OcctModelShape secondFace,
        OcctModelShape sharedEdge,
        int sampleCount = 32,
        OcctCurveContinuityOptions? options = null)
    {
        EnsureShape(firstFace);
        EnsureShape(secondFace);
        EnsureShape(sharedEdge);
        if (sampleCount < 2) throw new ArgumentOutOfRangeException(nameof(sampleCount));
        var actual = options ?? OcctCurveContinuityOptions.Default;
        ValidateContinuityOptions(actual);
        var native = actual.ToNative();
        var status = ModelNativeMethods.occt_model_surface_continuity_analyze(
            _handle, firstFace.Id, secondFace.Id, sharedEdge.Id, sampleCount,
            in native, out var result);
        CheckStatus(status);
        return result.ToManaged();
    }

    public IReadOnlyList<OcctCurvatureCombSample> SampleCurvatureComb(
        OcctModelShape edge,
        int sampleCount = 64,
        double scale = 1.0,
        double resolution = 1e-7)
    {
        EnsureShape(edge);
        if (sampleCount < 2 || sampleCount > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(sampleCount));
        if (!double.IsFinite(scale) || scale < 0) throw new ArgumentOutOfRangeException(nameof(scale));
        if (!double.IsFinite(resolution) || resolution <= 0)
            throw new ArgumentOutOfRangeException(nameof(resolution));

        CheckStatus(ModelNativeMethods.occt_model_curvature_comb_copy(
            _handle, edge.Id, sampleCount, scale, resolution, null, 0, out var required));
        var native = new NativeModelCurvatureCombSample[required];
        CheckStatus(ModelNativeMethods.occt_model_curvature_comb_copy(
            _handle, edge.Id, sampleCount, scale, resolution,
            native, native.Length, out var written));
        if (written != native.Length)
            throw new InvalidOperationException("Native curvature-comb sample count changed during copy.");
        return Array.ConvertAll(native, value => value.ToManaged());
    }

    public OcctSurfaceQualityAnalysis AnalyzeSurfaceQuality(
        OcctModelShape face,
        OcctSurfaceQualityOptions? options = null)
    {
        EnsureShape(face);
        var actual = options ?? OcctSurfaceQualityOptions.Default;
        ValidateSurfaceQualityOptions(actual);
        var nativeOptions = actual.ToNative();
        CheckStatus(ModelNativeMethods.occt_model_surface_quality_copy(
            _handle, face.Id, in nativeOptions, null, 0,
            out var required, out _));
        var native = new NativeModelSurfaceQualitySample[required];
        CheckStatus(ModelNativeMethods.occt_model_surface_quality_copy(
            _handle, face.Id, in nativeOptions, native, native.Length,
            out var written, out var summary));
        if (written != native.Length)
            throw new InvalidOperationException("Native surface-quality sample count changed during copy.");
        return new OcctSurfaceQualityAnalysis(
            summary.ToManaged(),
            Array.ConvertAll(native, value => value.ToManaged()));
    }

    private static void ValidateSurfaceQualityOptions(OcctSurfaceQualityOptions options)
    {
        if (options.USamples < 2 || options.VSamples < 2 ||
            options.USamples > 4096 || options.VSamples > 4096)
            throw new ArgumentOutOfRangeException(
                nameof(options), "U and V sample counts must be between 2 and 4096.");
        if (!double.IsFinite(options.Resolution) || options.Resolution <= 0 ||
            !double.IsFinite(options.ZebraFrequency) || options.ZebraFrequency <= 0 ||
            !double.IsFinite(options.ZebraPhase) ||
            !double.IsFinite(options.ViewDirection.X) ||
            !double.IsFinite(options.ViewDirection.Y) ||
            !double.IsFinite(options.ViewDirection.Z))
            throw new ArgumentOutOfRangeException(nameof(options), "Surface quality options are invalid.");
        var lengthSquared =
            options.ViewDirection.X * options.ViewDirection.X +
            options.ViewDirection.Y * options.ViewDirection.Y +
            options.ViewDirection.Z * options.ViewDirection.Z;
        if (lengthSquared <= 1e-30)
            throw new ArgumentException("Zebra view direction must not be zero.", nameof(options));
    }
}
