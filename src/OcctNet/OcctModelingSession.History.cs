using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public IReadOnlyList<OcctModelShape> GetGeneratedShapes(long operationId, OcctModelShape source)
    {
        EnsureShape(source);
        var count = ModelNativeMethods.occt_model_history_generated_count(_handle, operationId, source.Id);
        return Enumerable.Range(0, count)
            .Select(index => CheckShape(ModelNativeMethods.occt_model_history_generated_at(_handle, operationId, source.Id, index)))
            .ToArray();
    }

    public IReadOnlyList<OcctModelShape> GetModifiedShapes(long operationId, OcctModelShape source)
    {
        EnsureShape(source);
        var count = ModelNativeMethods.occt_model_history_modified_count(_handle, operationId, source.Id);
        return Enumerable.Range(0, count)
            .Select(index => CheckShape(ModelNativeMethods.occt_model_history_modified_at(_handle, operationId, source.Id, index)))
            .ToArray();
    }

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
}
