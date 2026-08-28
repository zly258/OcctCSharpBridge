namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public IReadOnlyList<OcctCurveCurveExtremum> GetEdgeExtrema(
        OcctModelShape firstEdge,
        OcctModelShape secondEdge)
    {
        EnsureShape(firstEdge);
        EnsureShape(secondEdge);

        CheckStatus(ModelNativeMethods.occt_model_edge_extrema_snapshot_get(
            _handle, firstEdge.Id, secondEdge.Id, null, 0, out var count));
        if (count == 0) return Array.Empty<OcctCurveCurveExtremum>();

        var native = new NativeModelCurveCurveExtremum[count];
        CheckStatus(ModelNativeMethods.occt_model_edge_extrema_snapshot_get(
            _handle, firstEdge.Id, secondEdge.Id, native, native.Length, out var required));
        if (required != count)
            throw new InvalidOperationException("Native edge-extrema count changed during snapshot copy.");

        var result = new OcctCurveCurveExtremum[count];
        for (var index = 0; index < count; ++index)
            result[index] = native[index].ToManaged();
        return result;
    }
}
