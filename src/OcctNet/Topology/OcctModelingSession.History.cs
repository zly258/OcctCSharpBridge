namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public IReadOnlyList<OcctModelShape> GetGeneratedShapes(long operationId, OcctModelShape source) =>
        GetHistoryShapes(operationId, source, generated: true);

    public IReadOnlyList<OcctModelShape> GetModifiedShapes(long operationId, OcctModelShape source) =>
        GetHistoryShapes(operationId, source, generated: false);

    public bool IsRemoved(long operationId, OcctModelShape source)
    {
        EnsureShape(source);
        CheckStatus(ModelNativeMethods.occt_model_history_is_removed_get(
            _handle,
            operationId,
            source.Id,
            out var result));
        return result != 0;
    }

    public OcctTopologyHistorySummary GetTopologyHistorySummary(long operationId, OcctModelShape source)
    {
        EnsureShape(source);
        CheckStatus(ModelNativeMethods.occt_model_history_summary(
            _handle,
            operationId,
            source.Id,
            out var result));
        return result.ToManaged();
    }

    public string GetOperationReport(long operationId)
    {
        if (operationId <= 0) throw new ArgumentOutOfRangeException(nameof(operationId));
        return ReadOperationReport(operationId);
    }

    private IReadOnlyList<OcctModelShape> GetHistoryShapes(long operationId, OcctModelShape source, bool generated)
    {
        EnsureShape(source);
        if (operationId <= 0) throw new ArgumentOutOfRangeException(nameof(operationId));

        var status = generated
            ? ModelNativeMethods.occt_model_history_generated_snapshot_get(
                _handle,
                operationId,
                source.Id,
                null,
                0,
                out var count)
            : ModelNativeMethods.occt_model_history_modified_snapshot_get(
                _handle,
                operationId,
                source.Id,
                null,
                0,
                out count);
        CheckStatus(status);
        if (count == 0) return Array.Empty<OcctModelShape>();

        var ids = new long[count];
        status = generated
            ? ModelNativeMethods.occt_model_history_generated_snapshot_get(
                _handle,
                operationId,
                source.Id,
                ids,
                ids.Length,
                out var required)
            : ModelNativeMethods.occt_model_history_modified_snapshot_get(
                _handle,
                operationId,
                source.Id,
                ids,
                ids.Length,
                out required);
        CheckStatus(status);
        if (required != count)
            throw new InvalidOperationException("Native topology-history count changed during snapshot copy.");

        var result = new OcctModelShape[count];
        for (var index = 0; index < count; index++)
            result[index] = CheckShape(ids[index]);
        return result;
    }
}
