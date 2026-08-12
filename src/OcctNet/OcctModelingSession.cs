using System.Runtime.InteropServices;
using System.Threading;

namespace OcctNet;

/// <summary>
/// Headless OCCT modeling session. No HWND, AIS context, or viewer is required.
/// </summary>
public sealed partial class OcctModelingSession : IDisposable
{
    private IntPtr _handle;

    public OcctModelingSession()
    {
        OcctRuntime.Configure();
        OcctBridgeInfo.EnsureCompatible();
        _handle = ModelNativeMethods.occt_model_create();
        if (_handle == IntPtr.Zero)
            throw new OcctException("Unable to create the native OCCT modeling session.");
    }

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
            return ModelNativeMethods.occt_model_shape_count(_handle);
        }
    }

    public IReadOnlyList<OcctModelShape> Shapes
    {
        get
        {
            EnsureNotDisposed();
            return Enumerable.Range(0, ShapeCount)
                .Select(index => ModelNativeMethods.occt_model_shape_id_at(_handle, index))
                .Where(id => id > 0)
                .Select(id => new OcctModelShape(id))
                .ToArray();
        }
    }

    public bool Exists(OcctModelShape shape)
    {
        EnsureNotDisposed();
        return shape.IsValid && ModelNativeMethods.occt_model_shape_exists(_handle, shape.Id) != 0;
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

    private delegate long ImportCall(IntPtr handle, string path);

    private OcctModelShape ImportSpecific(string filePath, ImportCall call)
    {
        ValidatePath(filePath);
        return CheckShape(call(_handle, Path.GetFullPath(filePath)));
    }

    private delegate int ExportCall(IntPtr handle, long shapeId, string path);

    private void ExportShape(OcctModelShape shape, string filePath, ExportCall call)
    {
        EnsureShape(shape);
        ValidatePath(filePath);
        Check(call(_handle, shape.Id, Path.GetFullPath(filePath)));
    }

    private static void ValidatePath(string path) => ArgumentException.ThrowIfNullOrWhiteSpace(path);

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
        if (!shape.IsValid || ModelNativeMethods.occt_model_shape_exists(_handle, shape.Id) == 0)
            throw new ArgumentException("Shape does not belong to this modeling session.", nameof(shape));
    }

    private OcctModelShape CheckShape(long id)
    {
        if (id <= 0) throw CreateException();
        return new OcctModelShape(id);
    }

    private OcctModelAlgorithmResult CheckAlgorithm(NativeModelAlgorithmResult native)
    {
        if (native.Succeeded == 0 || native.ShapeId <= 0) throw CreateException();
        return new OcctModelAlgorithmResult(this, native);
    }

    private void Check(int result)
    {
        if (result == 0) throw CreateException();
    }

    private OcctException CreateException()
    {
        var pointer = _handle == IntPtr.Zero
            ? IntPtr.Zero
            : ModelNativeMethods.occt_model_last_error(_handle);
        var message = pointer == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(pointer);
        return new OcctException(string.IsNullOrWhiteSpace(message)
            ? "The native OCCT modeling operation failed."
            : message);
    }

    private void EnsureNotDisposed() =>
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _handle) == IntPtr.Zero, this);

    public void Dispose()
    {
        ReleaseHandle(throwOnError: true);
        GC.SuppressFinalize(this);
    }

    private void ReleaseHandle(bool throwOnError)
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle == IntPtr.Zero) return;

        if (throwOnError)
        {
            ModelNativeMethods.occt_model_destroy(handle);
            return;
        }

        try
        {
            ModelNativeMethods.occt_model_destroy(handle);
        }
        catch
        {
            // Finalizers must not allow native unload failures to terminate the process.
        }
    }

    ~OcctModelingSession() => ReleaseHandle(throwOnError: false);
}
