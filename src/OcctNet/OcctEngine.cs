using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace OcctNet;

public sealed partial class OcctEngine : IDisposable
{
    private static long s_nextOwnerId;

    private readonly long _ownerId = Interlocked.Increment(ref s_nextOwnerId);
    private readonly OcctEngineSafeHandle _handle;
    private bool _initialized;

    public OcctEngine()
    {
        OcctRuntime.Configure();
        OcctBridgeInfo.EnsureCompatible();

        var safeHandle = OcctEngineSafeHandle.AdoptOwned(NativeMethods.occt_engine_create());
        if (safeHandle.IsInvalid)
        {
            safeHandle.Dispose();
            throw new OcctException("Unable to create the native OCCT engine.", OcctStatus.ErrorUnknown, nameof(OcctEngine));
        }
        _handle = safeHandle;
    }

    internal long OwnerId => _ownerId;

    public bool IsDisposed => _handle.IsClosed || _handle.IsInvalid;

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

    private OcctOverlay CheckOverlay(
        long id,
        OcctOverlayPrimitiveType primitiveType,
        [CallerMemberName] string? operation = null)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctOverlay(id, _ownerId, primitiveType);
    }

    private OcctManipulator CheckManipulator(long id, [CallerMemberName] string? operation = null)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctManipulator(id, _ownerId);
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
        var (status, nativeMessage) = NativeError.ReadEngine(_handle);
        var message = string.IsNullOrWhiteSpace(nativeMessage)
            ? "The native OCCT operation failed."
            : nativeMessage;
        return new OcctException(message, status, operation, nativeMessage);
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

    private void EnsureOverlay(OcctOverlay overlay, OcctOverlayPrimitiveType? primitiveType = null)
    {
        EnsureObject(overlay);
        if (NativeMethods.occt_object_kind(_handle, overlay.Id) != (int)OcctObjectKind.Overlay)
            throw new ArgumentException("Object is not an overlay object in this OcctEngine.", nameof(overlay));
        if (primitiveType is { } expected && overlay.PrimitiveType != expected)
            throw new ArgumentException($"Overlay must be of type {expected}.", nameof(overlay));
    }

    private void EnsureManipulator(OcctManipulator manipulator)
    {
        EnsureObject(manipulator);
        if (NativeMethods.occt_object_kind(_handle, manipulator.Id) != (int)OcctObjectKind.Manipulator)
            throw new ArgumentException("Object is not a manipulator in this OcctEngine.", nameof(manipulator));
    }

    private static long GetOwnerId(IOcctObject value) => value switch
    {
        OcctShape item => item.OwnerId,
        OcctText item => item.OwnerId,
        OcctDimension item => item.OwnerId,
        OcctPoint item => item.OwnerId,
        OcctOverlay item => item.OwnerId,
        OcctManipulator item => item.OwnerId,
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
        _handle.Dispose();
    }
}
