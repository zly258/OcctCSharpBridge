using System.Runtime.InteropServices;
using System.Text;

namespace OcctNet;

/// <summary>
/// Owns an immutable snapshot of native algorithm diagnostics independently from its source modeling session.
/// </summary>
public sealed class OcctAlgorithmResource : IDisposable
{
    private readonly OcctAlgorithmSafeHandle _handle;
    private readonly NativeAlgorithmSummary _summary;

    internal OcctAlgorithmResource(OcctAlgorithmSafeHandle handle)
    {
        _handle = handle;
        var summary = new NativeAlgorithmSummary
        {
            StructSize = (uint)Marshal.SizeOf<NativeAlgorithmSummary>(),
            ApiVersion = 1
        };
        var status = ModelNativeMethods.occt_algorithm_get_summary(_handle, ref summary);
        if (status != OcctStatus.Ok)
            throw new OcctException("Unable to query the owned native algorithm result.", status, nameof(OcctAlgorithmResource));
        _summary = summary;
    }

    public bool IsDisposed => _handle.IsClosed || _handle.IsInvalid;
    public long OperationId => _summary.OperationId;
    public bool HasWarnings => _summary.HasWarnings != 0;
    public bool HasErrors => _summary.HasErrors != 0;

    public string Report
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            var status = ModelNativeMethods.occt_algorithm_report_copy(_handle, null, 0, out var required);
            if (status != OcctStatus.Ok && status != OcctStatus.ErrorBufferTooSmall)
                throw new OcctException("Unable to query the native algorithm report size.", status, nameof(Report));
            if (required <= 0) return string.Empty;

            var buffer = new byte[required];
            status = ModelNativeMethods.occt_algorithm_report_copy(_handle, buffer, buffer.Length, out var written);
            if (status != OcctStatus.Ok)
                throw new OcctException("Unable to copy the native algorithm report.", status, nameof(Report));
            return Encoding.UTF8.GetString(buffer, 0, written);
        }
    }

    public void Dispose() => _handle.Dispose();
}
