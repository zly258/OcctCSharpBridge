using System.Runtime.InteropServices;

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
        return ModelNativeMethods.occt_model_history_is_removed(_handle, operationId, source.Id) != 0;
    }

    public string GetOperationReport(long operationId)
    {
        EnsureNotDisposed();
        return Marshal.PtrToStringUTF8(ModelNativeMethods.occt_model_operation_report(_handle, operationId)) ?? string.Empty;
    }

    private IReadOnlyList<OcctModelShape> GetHistoryShapes(long operationId, OcctModelShape source, bool generated)
    {
        EnsureShape(source);
        var count = generated
            ? ModelNativeMethods.occt_model_history_generated_count(_handle, operationId, source.Id)
            : ModelNativeMethods.occt_model_history_modified_count(_handle, operationId, source.Id);
        if (count <= 0) return Array.Empty<OcctModelShape>();

        var ids = new long[count];
        var copied = generated
            ? ModelNativeMethods.occt_model_history_generated_copy(_handle, operationId, source.Id, ids, ids.Length)
            : ModelNativeMethods.occt_model_history_modified_copy(_handle, operationId, source.Id, ids, ids.Length);
        if (copied < 0) throw CreateException();
        if (copied != count) throw new InvalidOperationException("Native topology-history count changed during bulk copy.");

        var result = new OcctModelShape[count];
        for (var index = 0; index < count; index++)
            result[index] = CheckShape(ids[index]);
        return result;
    }
}
