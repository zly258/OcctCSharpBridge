using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace OcctNet;

public sealed partial class OcctEngine : IDisposable
{
    private static long s_nextOwnerId;

    private readonly long _ownerId = Interlocked.Increment(ref s_nextOwnerId);
    private readonly OcctEngineSafeHandle _safeHandle;
    private IntPtr _handle;
    private bool _initialized;

    public OcctEngine()
    {
        OcctRuntime.Configure();
        OcctBridgeInfo.EnsureCompatible();

        var nativeHandle = NativeMethods.occt_create();
        if (nativeHandle == IntPtr.Zero)
            throw new OcctException("Unable to create the native OCCT engine.", nameof(OcctEngine));

        _safeHandle = new OcctEngineSafeHandle(nativeHandle);
        _handle = nativeHandle;
    }

    internal long OwnerId => _ownerId;

    public bool IsDisposed => Volatile.Read(ref _handle) == IntPtr.Zero || _safeHandle.IsClosed;

    public bool IsInitialized =>
        Volatile.Read(ref _initialized) &&
        !IsDisposed;

    public static string OcctVersion => OcctBridgeInfo.OcctVersion;

    private static void ValidatePath(string path) =>
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

    private OcctShape CheckShape(long id, [CallerMemberName] string? operation = null)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctShape(id, _ownerId);
    }

    private OcctText CheckText(long id, [CallerMemberName] string? operation = null)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctText(id, _ownerId);
    }

    private OcctDimension CheckDimension(long id, [CallerMemberName] string? operation = null)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctDimension(id, _ownerId);
    }

    private void CheckInitialized(Func<int> nativeCall, [CallerMemberName] string? operation = null)
    {
        EnsureInitialized();
        Check(nativeCall(), operation);
    }

    private void Check(int result, [CallerMemberName] string? operation = null)
    {
        if (result == 0) throw CreateException(operation);
    }

    private OcctException CreateException(string? operation = null)
    {
        var pointer = _handle == IntPtr.Zero
            ? IntPtr.Zero
            : NativeMethods.occt_last_error(_handle);
        var nativeMessage = pointer == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(pointer);
        var message = string.IsNullOrWhiteSpace(nativeMessage)
            ? "The native OCCT operation failed."
            : nativeMessage;
        return new OcctException(message, operation, nativeMessage);
    }

    private void EnsureObject(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureNotDisposed();
        if (GetOwnerId(value) != _ownerId)
            throw new ArgumentException("Object does not belong to this OcctEngine.", nameof(value));
        if (value.Id <= 0 || NativeMethods.occt_object_exists(_handle, value.Id) == 0)
            throw new ArgumentException("Object no longer exists in this OcctEngine.", nameof(value));
    }

    private void EnsureShape(OcctShape shape)
    {
        EnsureNotDisposed();
        if (!shape.IsValid || shape.OwnerId != _ownerId)
            throw new ArgumentException("Shape does not belong to this OcctEngine.", nameof(shape));
        if (NativeMethods.occt_object_exists(_handle, shape.Id) == 0 ||
            NativeMethods.occt_object_kind(_handle, shape.Id) != (int)OcctObjectKind.Shape)
        {
            throw new ArgumentException("Shape no longer exists in this OcctEngine.", nameof(shape));
        }
    }

    private void EnsureText(OcctText text)
    {
        EnsureObject(text);
        if (NativeMethods.occt_object_kind(_handle, text.Id) != (int)OcctObjectKind.Text)
            throw new ArgumentException("Object is not a text object in this OcctEngine.", nameof(text));
    }

    private void EnsureDimension(OcctDimension dimension)
    {
        EnsureObject(dimension);
        if (NativeMethods.occt_object_kind(_handle, dimension.Id) != (int)OcctObjectKind.Dimension)
            throw new ArgumentException("Object is not a dimension object in this OcctEngine.", nameof(dimension));
    }

    private static long GetOwnerId(IOcctObject value) => value switch
    {
        OcctShape item => item.OwnerId,
        OcctText item => item.OwnerId,
        OcctDimension item => item.OwnerId,
        _ => long.MinValue
    };

    private long[] GetObjectIds(IEnumerable<IOcctObject> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values);
        var ids = new HashSet<long>();
        foreach (var value in values)
        {
            ArgumentNullException.ThrowIfNull(value);
            try
            {
                EnsureObject(value);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException(exception.Message, parameterName, exception);
            }
            ids.Add(value.Id);
        }
        return ids.ToArray();
    }

    private void EnsureInitialized()
    {
        EnsureNotDisposed();
        if (!Volatile.Read(ref _initialized))
            throw new InvalidOperationException("Initialize the OCCT engine with a valid window handle first.");
    }

    private void EnsureNotDisposed() =>
        ObjectDisposedException.ThrowIf(IsDisposed, this);

    public void Dispose()
    {
        Volatile.Write(ref _initialized, false);
        if (Interlocked.Exchange(ref _handle, IntPtr.Zero) == IntPtr.Zero) return;
        _safeHandle.Dispose();
    }
}
