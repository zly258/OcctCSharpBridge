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

    public bool IsInitialized =>
        Volatile.Read(ref _initialized) &&
        Volatile.Read(ref _handle) != IntPtr.Zero &&
        !_safeHandle.IsClosed;

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
        if (IsForeignObject(value))
            throw new ArgumentException("Object belongs to a different OcctEngine.", nameof(value));
        if (value.Id <= 0 || NativeMethods.occt_object_exists(_handle, value.Id) == 0)
            throw new ArgumentException("Object does not belong to this OCCT engine.", nameof(value));
    }

    private void EnsureShape(OcctShape shape)
    {
        EnsureNotDisposed();
        if (shape.OwnerId != 0 && shape.OwnerId != _ownerId)
            throw new ArgumentException("Shape belongs to a different OcctEngine.", nameof(shape));
        if (!shape.IsValid ||
            NativeMethods.occt_object_exists(_handle, shape.Id) == 0 ||
            NativeMethods.occt_object_kind(_handle, shape.Id) != (int)OcctObjectKind.Shape)
        {
            throw new ArgumentException("Shape does not belong to this OCCT engine.", nameof(shape));
        }
    }

    private bool IsForeignObject(IOcctObject value)
    {
        var ownerId = GetOwnerId(value);
        return ownerId != 0 && ownerId != _ownerId;
    }

    private static long GetOwnerId(IOcctObject value) => value switch
    {
        OcctObject item => item.OwnerId,
        OcctShape item => item.OwnerId,
        OcctText item => item.OwnerId,
        OcctDimension item => item.OwnerId,
        _ => 0
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
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _handle) == IntPtr.Zero || _safeHandle.IsClosed,
            this);

    public void Dispose()
    {
        Volatile.Write(ref _initialized, false);
        if (Interlocked.Exchange(ref _handle, IntPtr.Zero) == IntPtr.Zero) return;
        _safeHandle.Dispose();
    }
}
