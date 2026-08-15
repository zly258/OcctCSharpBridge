using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void UpdatePoints(IReadOnlyList<OcctPointStateUpdate> updates)
    {
        ArgumentNullException.ThrowIfNull(updates);
        EnsureInitialized();
        if (updates.Count == 0) return;

        var native = new NativeOcctPointStateUpdate[updates.Count];
        for (var index = 0; index < updates.Count; index++)
        {
            var update = updates[index];
            EnsurePoint(update.Point);
            OcctGuard.Finite(update.Position, nameof(updates));
            native[index] = new NativeOcctPointStateUpdate
            {
                PointId = update.Point.Id,
                Position = update.Position,
                Visible = update.Visible ? 1 : 0
            };
        }

        GCHandle pinned = default;
        try
        {
            pinned = GCHandle.Alloc(native, GCHandleType.Pinned);
            var status = PointNativeMethods.occt_engine_points_update(
                _handle,
                pinned.AddrOfPinnedObject(),
                native.Length);
            if (status != OcctStatus.Ok) throw CreateException();
        }
        finally
        {
            if (pinned.IsAllocated) pinned.Free();
        }
    }
}
