using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    private delegate OcctStatus GeometryPointArrayCall(IntPtr points, int count, out long result);
    private delegate OcctStatus GeometryIdArrayCall(IntPtr ids, int count, out long result);

    private OcctShape GeometryResult(OcctStatus status, long result)
    {
        if (status != OcctStatus.Ok) throw CreateException();
        return CheckShape(result);
    }

    private OcctShape CreateGeometryFromPoints(OcctPoint3d[] points, GeometryPointArrayCall call)
    {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentNullException.ThrowIfNull(call);
        if (points.Length == 0) throw new ArgumentException("Point collection must not be empty.", nameof(points));

        var pin = GCHandle.Alloc(points, GCHandleType.Pinned);
        try
        {
            var status = call(pin.AddrOfPinnedObject(), points.Length, out var result);
            return GeometryResult(status, result);
        }
        finally
        {
            pin.Free();
        }
    }

    private OcctShape CreateGeometryFromIds(long[] ids, GeometryIdArrayCall call)
    {
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(call);
        if (ids.Length == 0) throw new ArgumentException("Shape collection must not be empty.", nameof(ids));

        var buffer = Marshal.AllocHGlobal(checked(sizeof(long) * ids.Length));
        try
        {
            Marshal.Copy(ids, 0, buffer, ids.Length);
            var status = call(buffer, ids.Length, out var result);
            return GeometryResult(status, result);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
