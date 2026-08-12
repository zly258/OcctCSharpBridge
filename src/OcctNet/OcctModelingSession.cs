using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace OcctNet;

/// <summary>
/// Headless OCCT modeling session. No HWND, AIS context, or viewer is required.
/// </summary>
public sealed partial class OcctModelingSession : IDisposable
{
    private static long s_nextOwnerId;

    private readonly long _ownerId = Interlocked.Increment(ref s_nextOwnerId);
    private readonly OcctModelingSafeHandle _safeHandle;
    private IntPtr _handle;

    public OcctModelingSession()
    {
        OcctRuntime.Configure();
        OcctBridgeInfo.EnsureCompatible();

        var nativeHandle = ModelNativeMethods.occt_model_create();
        if (nativeHandle == IntPtr.Zero)
            throw new OcctException("Unable to create the native OCCT modeling session.", nameof(OcctModelingSession));

        _safeHandle = new OcctModelingSafeHandle(nativeHandle);
        _handle = nativeHandle;
    }

    internal long OwnerId => _ownerId;

    public bool IsDisposed => Volatile.Read(ref _handle) == IntPtr.Zero || _safeHandle.IsClosed;

    internal IntPtr NativeHandle
    {
        get
        {
            EnsureNotDisposed();
            return _handle;
        }
    }

    public static string Capabilities =>
        Marshal.PtrToStringUTF8(ModelNativeMethods.occt_model_capabilities()) ?? string.Empty;

    public int ShapeCount
    {
        get
        {
            EnsureNotDisposed();
            return ModelNativeMethods.occt_model_shape_ids_copy(_handle, null, 0);
        }
    }

    public IReadOnlyList<OcctModelShape> Shapes
    {
        get
        {
            EnsureNotDisposed();
            var count = ModelNativeMethods.occt_model_shape_ids_copy(_handle, null, 0);
            if (count < 0) throw CreateException();
            if (count == 0) return Array.Empty<OcctModelShape>();

            var ids = new long[count];
            var copied = ModelNativeMethods.occt_model_shape_ids_copy(_handle, ids, ids.Length);
            if (copied < 0) throw CreateException();
            if (copied != count) throw new InvalidOperationException("Native shape count changed during bulk copy.");

            var result = new OcctModelShape[count];
            for (var index = 0; index < count; index++)
                result[index] = new OcctModelShape(ids[index], _ownerId);
            return result;
        }
    }

    public bool Exists(OcctModelShape shape)
    {
        EnsureNotDisposed();
        return shape.IsValid &&
               shape.OwnerId == _ownerId &&
               ModelNativeMethods.occt_model_shape_exists(_handle, shape.Id) != 0;
    }

    public bool Owns(OcctModelShape shape) => shape.IsValid && shape.OwnerId == _ownerId;

    public OcctModelShape GetShape(long id)
    {
        EnsureNotDisposed();
        if (id <= 0 || ModelNativeMethods.occt_model_shape_exists(_handle, id) == 0)
            throw new ArgumentOutOfRangeException(nameof(id), id, "The shape ID does not exist in this modeling session.");
        return new OcctModelShape(id, _ownerId);
    }

    public bool TryGetShape(long id, out OcctModelShape shape)
    {
        EnsureNotDisposed();
        if (id > 0 && ModelNativeMethods.occt_model_shape_exists(_handle, id) != 0)
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
        Check(ModelNativeMethods.occt_model_delete_shape(_handle, shape.Id));
    }

    public void Clear() => Check(ModelNativeMethods.occt_model_clear(NativeHandle));

    public OcctModelShape Copy(OcctModelShape shape)
    {
        EnsureShape(shape);
        return CheckShape(ModelNativeMethods.occt_model_copy_shape(_handle, shape.Id));
    }

    private delegate int PropertyCall(IntPtr handle, long id, out OcctMassProperties result);

    private OcctMassProperties GetProperties(OcctModelShape shape, PropertyCall call)
    {
        EnsureShape(shape);
        Check(call(_handle, shape.Id, out var result));
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
        foreach (var shape in array) EnsureShape(shape);
        return array.Select(shape => shape.Id).ToArray();
    }

    private void EnsureShape(OcctModelShape shape)
    {
        EnsureNotDisposed();
        if (!shape.IsValid || shape.OwnerId != _ownerId)
            throw new ArgumentException("Shape does not belong to this modeling session.", nameof(shape));
        if (ModelNativeMethods.occt_model_shape_exists(_handle, shape.Id) == 0)
            throw new ArgumentException("Shape no longer exists in this modeling session.", nameof(shape));
    }

    private OcctModelShape CheckShape(long id, [CallerMemberName] string? operation = null)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctModelShape(id, _ownerId);
    }

    private OcctModelAlgorithmResult CheckAlgorithm(NativeModelAlgorithmResult native, [CallerMemberName] string? operation = null)
    {
        if (native.Succeeded == 0 || native.ShapeId <= 0) throw CreateException(operation);
        return new OcctModelAlgorithmResult(this, native);
    }

    private void Check(int result, [CallerMemberName] string? operation = null)
    {
        if (result == 0) throw CreateException(operation);
    }

    private OcctException CreateException(string? operation = null)
    {
        var pointer = _handle == IntPtr.Zero
            ? IntPtr.Zero
            : ModelNativeMethods.occt_model_last_error(_handle);
        var nativeMessage = pointer == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(pointer);
        var message = string.IsNullOrWhiteSpace(nativeMessage)
            ? "The native OCCT modeling operation failed."
            : nativeMessage;
        return new OcctException(message, operation, nativeMessage);
    }

    private void EnsureNotDisposed() => ObjectDisposedException.ThrowIf(IsDisposed, this);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _handle, IntPtr.Zero) == IntPtr.Zero) return;
        _safeHandle.Dispose();
    }
}
