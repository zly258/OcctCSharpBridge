using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace OcctNet;

/// <summary>
/// Headless OCCT modeling session. No HWND, AIS context, or viewer is required.
/// </summary>
public sealed partial class OcctModelingSession : IDisposable
{
    private static long s_nextOwnerId;

    private readonly long _ownerId = Interlocked.Increment(ref s_nextOwnerId);
    private readonly OcctModelingSafeHandle _handle;

    public OcctModelingSession()
    {
        OcctRuntime.Configure();
        OcctBridgeInfo.EnsureCompatible();

        var safeHandle = OcctModelingSafeHandle.AdoptOwned(ModelNativeMethods.occt_model_session_create());
        if (safeHandle.IsInvalid)
        {
            safeHandle.Dispose();
            throw new OcctException(
                "Unable to create the native OCCT modeling session.",
                OcctStatus.ErrorUnknown,
                nameof(OcctModelingSession));
        }
        _handle = safeHandle;
    }

    internal long OwnerId => _ownerId;

    public bool IsDisposed => _handle.IsClosed || _handle.IsInvalid;

    internal OcctModelingSafeHandle NativeHandle
    {
        get
        {
            EnsureNotDisposed();
            return _handle;
        }
    }

    public static string Capabilities => ReadCapabilities();

    public int ShapeCount
    {
        get
        {
            EnsureNotDisposed();
            var status = ModelNativeMethods.occt_model_shapes_snapshot_get(_handle, null, 0, out var required);
            if (status != OcctStatus.Ok)
                throw new OcctException("Unable to query modeling shape count.", status, nameof(ShapeCount));
            return required;
        }
    }

    public IReadOnlyList<OcctModelShape> Shapes
    {
        get
        {
            EnsureNotDisposed();
            var status = ModelNativeMethods.occt_model_shapes_snapshot_get(_handle, null, 0, out var count);
            if (status != OcctStatus.Ok) throw CreateException();
            if (count == 0) return Array.Empty<OcctModelShape>();

            var ids = new long[count];
            status = ModelNativeMethods.occt_model_shapes_snapshot_get(_handle, ids, ids.Length, out var required);
            if (status != OcctStatus.Ok) throw CreateException();
            if (required != count)
                throw new InvalidOperationException("Native shape count changed during bulk copy.");

            var result = new OcctModelShape[count];
            for (var index = 0; index < count; index++)
                result[index] = new OcctModelShape(ids[index], _ownerId);
            return result;
        }
    }

    public bool Exists(OcctModelShape shape)
    {
        EnsureNotDisposed();
        if (!shape.IsValid || shape.OwnerId != _ownerId) return false;
        var status = ModelNativeMethods.occt_model_shape_exists_get(_handle, shape.Id, out var exists);
        if (status != OcctStatus.Ok) throw CreateException();
        return exists != 0;
    }

    public bool Owns(OcctModelShape shape) => shape.IsValid && shape.OwnerId == _ownerId;

    public OcctModelShape GetShape(long id)
    {
        EnsureNotDisposed();
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        var status = ModelNativeMethods.occt_model_shape_exists_get(_handle, id, out var exists);
        if (status != OcctStatus.Ok) throw CreateException();
        if (exists == 0)
            throw new ArgumentOutOfRangeException(nameof(id), id, "The shape ID does not exist in this modeling session.");
        return new OcctModelShape(id, _ownerId);
    }

    public bool TryGetShape(long id, out OcctModelShape shape)
    {
        EnsureNotDisposed();
        if (id <= 0)
        {
            shape = default;
            return false;
        }

        var status = ModelNativeMethods.occt_model_shape_exists_get(_handle, id, out var exists);
        if (status != OcctStatus.Ok) throw CreateException();
        if (exists != 0)
        {
            shape = new OcctModelShape(id, _ownerId);
            return true;
        }

        shape = default;
        return false;
    }

    public void Delete(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_delete(_handle, shape.Id));
    }

    public void Clear() => CheckStatus(ModelNativeMethods.occt_model_session_clear(NativeHandle));

    public OcctModelShape Copy(OcctModelShape shape)
    {
        EnsureShape(shape);
        var status = ModelNativeMethods.occt_model_shape_copy(_handle, shape.Id, out var result);
        CheckStatus(status);
        return CheckShape(result);
    }

    private delegate OcctStatus PropertyCall(OcctModelingSafeHandle handle, long id, out OcctMassProperties result);

    private OcctMassProperties GetProperties(OcctModelShape shape, PropertyCall call)
    {
        EnsureShape(shape);
        CheckStatus(call(_handle, shape.Id, out var result));
        return result;
    }

    private static T[] RequiredArray<T>(IEnumerable<T> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var result = values.ToArray();
        if (result.Length == 0)
            throw new ArgumentException("Collection must not be empty.", parameterName);
        return result;
    }

    private long[] ShapeIds(IEnumerable<OcctModelShape> shapes)
    {
        var array = RequiredArray(shapes, nameof(shapes));
        var ids = new long[array.Length];
        for (var i = 0; i < array.Length; i++)
        {
            EnsureShape(array[i]);
            ids[i] = array[i].Id;
        }
        return ids;
    }

    private void EnsureShape(OcctModelShape shape)
    {
        EnsureNotDisposed();
        if (!shape.IsValid || shape.OwnerId != _ownerId)
            throw new ArgumentException("Shape does not belong to this modeling session.", nameof(shape));

        var status = ModelNativeMethods.occt_model_shape_exists_get(_handle, shape.Id, out var exists);
        if (status != OcctStatus.Ok) throw CreateException();
        if (exists == 0)
            throw new ArgumentException("Shape no longer exists in this modeling session.", nameof(shape));
    }

    private OcctModelShape CheckShape(long id, [CallerMemberName] string? operation = null)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctModelShape(id, _ownerId);
    }

    private OcctModelShape CheckShape(OcctStatus status, long id, [CallerMemberName] string? operation = null)
    {
        CheckStatus(status, operation);
        return CheckShape(id, operation);
    }

    private OcctModelAlgorithmResult CheckAlgorithm(NativeModelAlgorithmResult native, [CallerMemberName] string? operation = null)
    {
        if (native.Succeeded == 0 || native.ShapeId <= 0) throw CreateException(operation);
        return new OcctModelAlgorithmResult(this, native);
    }

    private OcctModelAlgorithmResult CheckAlgorithm(
        OcctStatus status,
        NativeModelAlgorithmResult native,
        [CallerMemberName] string? operation = null)
    {
        CheckStatus(status, operation);
        return CheckAlgorithm(native, operation);
    }

    private void CheckStatus(OcctStatus status, [CallerMemberName] string? operation = null)
    {
        if (status != OcctStatus.Ok) throw CreateException(operation);
    }

    internal string ReadOperationReport(long operationId)
    {
        EnsureNotDisposed();
        return ReadUtf8Buffer((byte[]? buffer, int capacity, out int required) =>
            ModelNativeMethods.occt_model_operation_report_get(_handle, operationId, buffer, capacity, out required));
    }

    private OcctException CreateException(string? operation = null)
    {
        var (status, nativeMessage) = NativeError.ReadModelingSession(_handle);
        var message = string.IsNullOrWhiteSpace(nativeMessage)
            ? "The native OCCT modeling operation failed."
            : nativeMessage;
        return new OcctException(message, status, operation, nativeMessage);
    }

    private static string ReadCapabilities() => ReadUtf8Buffer((byte[]? buffer, int capacity, out int required) =>
        ModelNativeMethods.occt_model_capabilities_get(buffer, capacity, out required));

    private delegate OcctStatus Utf8BufferCall(byte[]? buffer, int capacity, out int required);

    private static string ReadUtf8Buffer(Utf8BufferCall call)
    {
        var status = call(null, 0, out var required);
        if (status != OcctStatus.Ok)
            throw new OcctException("Unable to query native UTF-8 buffer size.", status);
        if (required <= 1) return string.Empty;

        // Use ArrayPool to avoid per-call heap allocation for UTF-8 string reads.
        var buffer = ArrayPool<byte>.Shared.Rent(required);
        try
        {
            status = call(buffer, required, out var copiedRequired);
            if (status != OcctStatus.Ok)
                throw new OcctException("Unable to read native UTF-8 buffer.", status);
            if (copiedRequired <= 0 || copiedRequired > required)
                throw new InvalidOperationException("Native UTF-8 buffer size is invalid.");
            return Encoding.UTF8.GetString(buffer, 0, copiedRequired - 1);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private void EnsureNotDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(OcctModelingSession), "The modeling session has been disposed.");
    }

    public void Dispose()
    {
        _handle.Dispose();
    }
}
